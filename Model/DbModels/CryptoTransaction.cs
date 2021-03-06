﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Model.Enums;

namespace Model.DbModels
{
    public class CryptoTransaction
    {
        private CryptoTransaction() { }

        // Fee is always added!
        /// <summary>
        /// News the trade.
        /// </summary>
        /// <param name="transactionKey">The transaction key.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="exchangeId">The exchange identifier.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="buyAmount">The buy amount.</param>
        /// <param name="buyCurrency">The buy currency.</param>
        /// <param name="fee">The fee.</param>
        /// <param name="feeCurrency">The fee currency.</param>
        /// <param name="sellAmount">Without fee</param>
        /// <param name="sellCurrency">The sell currency.</param>
        /// <param name="fiatRate">The fiat rate of the buying currency.</param>
        /// <returns>CryptoTransaction.</returns>
        public static CryptoTransaction NewTrade(string transactionKey, DateTime dateTime, Guid exchangeId, string comment, decimal buyAmount, string buyCurrency, decimal fee, string feeCurrency, decimal sellAmount, string sellCurrency, decimal fiatRate)
        {
            var t = new CryptoTransaction()
            {
                Type = TransactionType.Trade,
                TransactionKey = transactionKey,
                DateTime = dateTime,
                ExchangeId = exchangeId,
                Comment = comment,
                BuyAmount = buyAmount,
                BuyCurrency = buyCurrency,
                FeeAmount = fee,
                FeeCurrency = feeCurrency,
                SellAmount = sellAmount,
                SellCurrency = sellCurrency,
                Rate = sellAmount / buyAmount,
                BuyFiatRate = fiatRate,
                BuyFiatAmount = fiatRate * buyAmount
            };

            return t;
        }

        public static CryptoTransaction NewIn(string transactionKey, DateTime dateTime, Guid exchangeId, string comment, decimal inAmount, string inCurrency, string fromAddress, string toAddress, string transactionHash)
        {
            var t = new CryptoTransaction()
            {
                Type = TransactionType.In,
                TransactionKey = transactionKey,
                DateTime = dateTime,
                ExchangeId = exchangeId,
                Comment = comment,
                InAmount = inAmount,
                InCurrency = inCurrency,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                TransactionHash = transactionHash
            };
            return t;
        }

        public static CryptoTransaction NewOut(string transactionKey, DateTime dateTime, Guid exchangeId, string comment, decimal outAmount, string outCurrency, decimal fee, string feeCurrency, string fromAddress, string toAddress, string transactionHash)
        {
            var t = new CryptoTransaction()
            {
                Type = TransactionType.Out,
                TransactionKey = transactionKey,
                DateTime = dateTime,
                ExchangeId = exchangeId,
                Comment = comment,
                OutAmount = outAmount,
                OutCurrency = outCurrency,
                FeeAmount = fee,
                FeeCurrency = feeCurrency,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                TransactionHash = transactionHash
            };
            return t;
        }



        [Key]
        public Guid Id { get; private set; }

        public TransactionType Type { get; private set; }
        public DateTime DateTime { get; private set; }

        public decimal InAmount { get; private set; }
        public string InCurrency { get; private set; }

        public decimal OutAmount { get; private set; }
        public string OutCurrency { get; private set; }

        public decimal FeeAmount { get; private set; }
        public string FeeCurrency { get; private set; }

        public decimal BuyAmount { get; private set; }
        public string BuyCurrency { get; private set; }
        public decimal BuyFiatRate { get; set; }
        public decimal BuyFiatAmount { get; set; }
        public decimal SellAmount { get; private set; }
        public string SellCurrency { get; private set; }

        public decimal Rate { get; private set; }

        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        public Guid ExchangeId { get; private set; }
        public string Comment { get; private set; }
        public string TransactionKey { get; private set; }

        public string TransactionHash { get; private set; }
    }
}
