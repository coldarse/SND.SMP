import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError as _observableThrow, Observable } from 'rxjs';
import { PagedWalletsResultRequestDto, TopUpEWalletDto, UpdateWalletDto, WalletDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ApiException } from '../service-proxies';
import { ErrorMessage } from '../error-handling';

@Injectable()
export class WalletService {

    url = '';
    options_: any;

    constructor(private http: HttpClient, private errorMessage: ErrorMessage){
        this.url = AppConsts.remoteServiceBaseUrl;
        this.options_ = {
            headers: new HttpHeaders({
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            })
        };
    }

    // private handleError(error: HttpErrorResponse) {
    //     if (error.error instanceof ErrorEvent) {
    //       console.error('An error occurred:', error.error.message);
    //     }
    //     else {
    //       console.error(
    //         `Backend returned code ${error.status}, ` +
    //         `body was: ${error.error}`);
    //     }
    //     return this.throwException(error.error.message, error.status, "", this.options_);
    // }

    private throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
        if (result !== null && result !== undefined)
            return _observableThrow(result);
        else
            return _observableThrow(new ApiException(message, status, response, headers, null));
    }

    //Create Wallet
    create(body: WalletDto){
        return this.http.post(
            this.url + '/api/services/app/Wallet/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Update Wallet
    update(body: WalletDto){
        return this.http.put(
            this.url + '/api/services/app/Wallet/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Delete Wallet
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/Wallet/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get Wallet
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/Wallet/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All Wallets
    getAll(body: PagedWalletsResultRequestDto){
        let url_ = this.url + "/api/services/app/Wallet/GetAll?";

        if (body.keyword === null)
            throw new Error("The parameter 'keyword' cannot be null.");
        else if (body.keyword !== undefined)
            url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

        if (body.skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";


        url_ = url_.replace(/[?&]$/, "");


        return this.http.get(
            url_ + `&MaxResultCount=10`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }



    // Update E-Wallet Async
    updateEWalletAsync(body: UpdateWalletDto){
        return this.http.put(
            this.url + '/api/services/app/Wallet/UpdateEWallet',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    // Delete E-Wallet Async
    deleteEWalletAsync(body: WalletDto){
        return this.http.delete(
            this.url + `/api/services/app/Wallet/DeleteEWallet?Customer=${body.customer}&EWalletType=${body.eWalletType}&Currency=${body.currency}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    // Get E-Wallet Async
    getEWalletAsync(body: WalletDto){
        if(body.id != undefined){
            return this.http.get(
                this.url + `/api/services/app/Wallet/GetEWallet?Id=${body.id}`,
                this.options_
            ).pipe(
                retry(1),
                catchError(this.errorMessage.HandleErrorResponse),
            )
        }
        else{
            return this.http.get(
                this.url + `/api/services/app/Wallet/GetEWallet?Customer=${body.customer}&EWalletType=${body.eWalletType}&Currency=${body.currency}`,
                this.options_
            ).pipe(
                retry(1),
                catchError(this.errorMessage.HandleErrorResponse),
            )
        }
    }

    // Top Up EWallet
    topUpEWallet(body: TopUpEWalletDto){
        return this.http.post(
            this.url + '/api/services/app/Wallet/TopUpEWallet',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Manage Credit
    manageCredit(body: TopUpEWalletDto){
        return this.http.post(
            this.url + '/api/services/app/Wallet/ManageCredit',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    // Get All Wallets Async By Customer ID
    getAllWalletsAsync(code: string){
        return this.http.get(
            this.url + `/api/services/app/Wallet/GetAllWallets?code=${code}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    // Get All Wallet Detail
    getWalletDetail(body: PagedWalletsResultRequestDto){
        let url_ = this.url + "/api/services/app/Wallet/GetWalletDetail?";

        if (body.keyword === null)
            throw new Error("The parameter 'keyword' cannot be null.");
        else if (body.keyword !== undefined)
            url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

        if (body.skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";


        url_ = url_.replace(/[?&]$/, "");


        return this.http.get(
            url_ + `&MaxResultCount=10`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }
}
