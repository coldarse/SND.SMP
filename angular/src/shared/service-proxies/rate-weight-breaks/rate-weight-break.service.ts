import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedRateWeightBreakResultRequestDto, RateWeightBreakDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ErrorMessage } from '../error-handling';

@Injectable()
export class RateWeightBreakService {

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
    //     return throwError(() => new Error(error.error.message));
    // }

    //Create RateWeightBreak
    create(body: RateWeightBreakDto){
        return this.http.post(
            this.url + '/api/services/app/RateWeightBreak/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Update RateWeightBreak
    update(body: RateWeightBreakDto){
        return this.http.put(
            this.url + '/api/services/app/RateWeightBreak/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Delete RateWeightBreak
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/RateWeightBreak/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get RateWeightBreak
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/RateWeightBreak/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All RateWeightBreaks
    getAll(body: PagedRateWeightBreakResultRequestDto){
        let url_ = this.url + "/api/services/app/RateWeightBreak/GetAll?";

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

    //Get RateWeightBreak By Rate
    getRateWeightBreakByRate(rateId: number){
        return this.http.get(
            this.url + `/api/services/app/RateWeightBreak/GetRateWeightBreakByRate?rateid=${rateId}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Upload Excel for RateWeightBreak
    uploadRateWeightBreakFile(body: any){
        return this.http.post(
            this.url + '/api/services/app/RateWeightBreak/UploadRateWeightBreakFile',
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
}
