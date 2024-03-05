import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError as _observableThrow, Observable } from 'rxjs';
import { PagedRatesResultRequestDto, RateDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ApiException } from '../service-proxies';

@Injectable()
export class RateService {

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
        return this.throwException(error.error.message, error.status, "", this.options_);
    }

    private throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
        if (result !== null && result !== undefined)
            return _observableThrow(result);
        else
            return _observableThrow(new ApiException(message, status, response, headers, null));
    }

    //Create Rate
    create(body: RateDto){
        return this.http.post(
            this.url + '/api/services/app/Rate/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Update Rate
    update(body: RateDto){
        return this.http.put(
            this.url + '/api/services/app/Rate/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Delete Rate
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/Rate/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Rate
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/Rate/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get All Rates
    getAll(body: PagedRatesResultRequestDto){
        let url_ = this.url + "/api/services/app/Rate/GetAll?";

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

    //Get All Rates Without Pagination
    getRates(){
        return this.http.get(
            this.url + `/api/services/app/Rate/GetRates`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

}
