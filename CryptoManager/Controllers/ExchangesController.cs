﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundServices;
using CryptoManager.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DbModels;
using Model.DTOs;
using Model.Meta;
using Plugins;

namespace CryptoManager.Controllers
{
    [Produces("application/json")]
    [Route("api/Exchanges")]
    public class ExchangesController : Controller
    {
        private readonly CryptoContext _cryptoContext;
        private readonly IMapper _mapper;
        private IMarketData _marketData;

        public ExchangesController(CryptoContext cryptoContext, IMapper mapper, IMarketData marketData)
        {
            _cryptoContext = cryptoContext;
            _mapper = mapper;
            _marketData = marketData;
        }

        [HttpGet("availableExchanges")]
        public async Task<IEnumerable<ExchangeMeta>> GetAvaliableExchanges()
        {
            return await Task.Run(() =>
            {
                var type = typeof(IImporter);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
                var res = new List<ExchangeMeta>();
                foreach (var importer in types)
                {
                    var i = (IImporter)Activator.CreateInstance(importer, _marketData);
                    var meta = i.GetExchangeMeta();
                    res.Add(meta);
                }
                return res;
            });
        }

        // GET: api/Exchanges
        [HttpGet]
        public IEnumerable<ExchangeDTO> Get()
        {
            var data = _cryptoContext.Exchanges.OrderByDescending(e => e.ExchangeId.ToString());
            var res = _mapper.Map<IEnumerable<ExchangeDTO>>(data);
            return res;
        }


        // POST: api/Exchanges
        [HttpPost]
        public async Task AddNewImporter([FromBody]Exchange value)
        {
            // Validation
            var availableExchanges = await GetAvaliableExchanges();
            var exchangeMeta = availableExchanges.SingleOrDefault(e => e.ExchangeId == value.ExchangeId);

            if (exchangeMeta == null)
                throw new ArgumentException("No exchange Plugin exists for this exchange");

            if (exchangeMeta.SupportsPrivateKey && string.IsNullOrEmpty(value.PrivateKey))
                throw new ArgumentOutOfRangeException(nameof(value.PrivateKey), exchangeMeta.LabelPrivateKey + " was not provided");

            if (exchangeMeta.SupportsPublicKey && string.IsNullOrEmpty(value.PublicKey))
                throw new ArgumentOutOfRangeException(nameof(value.PublicKey), exchangeMeta.LabelPublicKey + " was not provided");

            if (exchangeMeta.SupportsPassphrase && string.IsNullOrEmpty(value.Passphrase))
                throw new ArgumentOutOfRangeException(nameof(value.Passphrase), exchangeMeta.LabelPassphrase + " was not provided");

            // Triming
            value.PublicKey = value.PublicKey?.Trim();
            value.PrivateKey = value.PrivateKey?.Trim();
            value.Passphrase = value.Passphrase?.Trim();
            value.Comment = value.Comment?.Trim();

            // Add Data
            await _cryptoContext.Exchanges.AddAsync(value);
            await _cryptoContext.SaveChangesAsync();

            // Schedule import
            BackgroundJob.Enqueue<Importer>(i => i.Import(value.Id, true));
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(Guid id)
        {
            var obj = await _cryptoContext.Exchanges.FindAsync(id);
            if (obj == null)
                return NotFound();

            _cryptoContext.Exchanges.Remove(obj);
            await _cryptoContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateExchange(Guid id)
        {
            // Check if exchange exits
            var entity = await _cryptoContext.Exchanges.FindAsync(id);
            if (entity == null)
                return NotFound();
            BackgroundJob.Enqueue<Importer>(i => i.Import(id, true));
            return Ok();
        }
    }
}
