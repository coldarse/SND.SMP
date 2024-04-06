import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError as _observableThrow, Observable } from 'rxjs';
import { PagedRateItemsResultRequestDto, RateItemDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ApiException } from '../service-proxies';
import { ErrorMessage } from '../error-handling';

@Injectable()
export class RateItemService {

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

    //Create RateItem
    create(body: RateItemDto){
        return this.http.post(
            this.url + '/api/services/app/RateItem/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Update RateItem
    update(body: RateItemDto){
        return this.http.put(
            this.url + '/api/services/app/RateItem/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Delete RateItem
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/RateItem/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get RateItem
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/RateItem/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All RateItems
    getAll(body: PagedRateItemsResultRequestDto){
        let url_ = this.url + "/api/services/app/RateItem/GetAll?";

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

    //Upload Excel for RateItem
    uploadRateItemFile(body: any){
        return this.http.post(
            this.url + '/api/services/app/RateItem/UploadRateItemFile',
            body,
            {
                headers: new HttpHeaders({
                    "Accept": "text/plain"
                })
            }
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All Rate Items Count
    getAllRateItemsCount(){
        return this.http.get(
            this.url + `/api/services/app/RateItem/GetAllRateItemsCount`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    // Get Full Rate Item Detail
    getFullRateItemDetail(body: PagedRateItemsResultRequestDto){
        let url_ = this.url + "/api/services/app/RateItem/GetFullRateItemDetail?";

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
