import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedBagResultRequestDto, BagDto } from './model';
import { AppConsts } from '@shared/AppConsts';

@Injectable()
export class BagService {

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

    //Create Bag
    create(body: BagDto){
        return this.http.post(
            this.url + '/api/services/app/Bag/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Update Bag
    update(body: BagDto){
        return this.http.put(
            this.url + '/api/services/app/Bag/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Delete Bag
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/Bag/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Bag
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/Bag/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get All Bags
    getAll(body: PagedBagResultRequestDto){
        let url_ = this.url + "/api/services/app/Bag/GetAll?";

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
}
