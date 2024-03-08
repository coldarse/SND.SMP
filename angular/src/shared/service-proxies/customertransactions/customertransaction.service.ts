import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedCustomerTransactionResultRequestDto, CustomerTransactionDto } from './model';
import { AppConsts } from '@shared/AppConsts';

@Injectable()
export class CustomerTransactionService {

    url = '';
    options_: any;

    constructor(private http: HttpClient){
        this.url = AppConsts.remoteServiceBaseUrl;
        this.options_ = {
            headers: new HttpHeaders({
                "Content-Type": "application/json-patch+json",
                "Accept": "text/plain"
            })
        };
    }

    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
          console.error('An error occurred:', error.error.message);
        }
        else {
          console.error(
            `Backend returned code ${error.status}, ` +
            `body was: ${error.error}`);
        }
        return throwError(() => new Error(error.error.message));
    }

    //Create Currency
    create(body: CustomerTransactionDto){
        return this.http.post(
            this.url + '/api/services/app/CustomerTransaction/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Update Currency
    update(body: CustomerTransactionDto){
        return this.http.put(
            this.url + '/api/services/app/CustomerTransaction/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Delete Currency
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/CustomerTransaction/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Currency
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/CustomerTransaction/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get All Currencies
    getAll(body: PagedCustomerTransactionResultRequestDto){
        let url_ = this.url + "/api/services/app/CustomerTransaction/GetAll?";

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
            catchError(this.handleError),
        )
    }

    // Get All CustomerTransaction without filter
    getCustomerTransaction(){
        return this.http.get(
            this.url + `/api/services/app/CustomerTransaction/GetCustomerTransactions`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }
}
