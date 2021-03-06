import { Component, OnInit } from '@angular/core';
import { SelectItem, ConfirmationService } from 'primeng/primeng';
import { CryptoApiClient, IExchangeMeta, Exchange, ExchangeDTO } from '../../services/api-client';


@Component({
    selector: 'app-exchanges',
    templateUrl: './exchanges.component.html',
    styleUrls: ['./exchanges.component.css'],
    providers: [ConfirmationService]
})

export class ExchangesComponent implements OnInit {
    availableExchanges: SelectItem[] = [{ label: 'Select Exchange Plugin', value: null }];
    selectedAvailableExchange: IExchangeMeta;

    publicKey: string;
    privateKey: string;
    passphrase: string;
    comment: string;
    showForm: boolean;

    ownExchanges: ExchangeDTO[];

    constructor(private apiClient: CryptoApiClient, private confirmationService: ConfirmationService) {

        apiClient.apiExchangesAvailableExchangesGet().subscribe(ex => {
            for (let entry of ex) {
                this.availableExchanges.push({
                    label: <string>entry.name,
                    value: entry
                });
            }
        });
    }

    addExchange() {
        let exchange = new Exchange({
            exchangeId: <any>(this.selectedAvailableExchange).exchangeId,
            comment: this.comment,
            privateKey: this.privateKey,
            publicKey: this.publicKey,
            passphrase: this.passphrase
        });
        console.log(exchange);
        this.apiClient.apiExchangesPost(exchange).subscribe(res => {
            this.comment = "";
            this.publicKey = "";
            this.privateKey = "";
            this.passphrase = "";
            this.showForm = false;
            this.refreshOwnExchanges();
        });
    }

    exchangeChanged() {
        if (this.selectedAvailableExchange == null) {
            this.showForm = false;
        } else {
            this.showForm = true;
        }
    }

    ngOnInit() {
        this.refreshOwnExchanges();
    }

    refreshOwnExchanges() {
        this.apiClient.apiExchangesGet().subscribe(res => this.ownExchanges = res);
    }


    remove(exchange: ExchangeDTO) {
        console.warn("Delete...");
        this.confirmationService.confirm({
            header: 'Delete',
            message: 'Do you really want to delete the connection to the exchange ' +
            <string>exchange.exchangeName +
            " (" +
            <string>exchange.comment +
            ")",
            accept: () => {
                this.apiClient.apiExchangesDelete((exchange.id) as string).subscribe(() => this.refreshOwnExchanges());
            }
        });
    }

    update(exchange: ExchangeDTO) {
        console.info("Updating...");
        this.apiClient.apiExchangesUpdatePost(String(exchange.id))
            .subscribe();
    }
}

