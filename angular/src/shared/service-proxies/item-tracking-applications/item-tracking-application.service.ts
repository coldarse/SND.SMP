import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedItemTrackingApplicationResultRequestDto, ItemTrackingApplicationDto } from './model';
import { AppConsts } from '@shared/AppConsts';

@Injectable()
export class ItemTrackingApplicationService {

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

    //Create ItemTrackingApplication
    create(body: ItemTrackingApplicationDto){
        return this.http.post(
            this.url + '/api/services/app/ItemTrackingApplication/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Update ItemTrackingApplication
    update(body: ItemTrackingApplicationDto){
        return this.http.put(
            this.url + '/api/services/app/ItemTrackingApplication/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Delete ItemTrackingApplication
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/ItemTrackingApplication/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get ItemTrackingApplication
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/ItemTrackingApplication/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get All ItemTrackingApplications
    getAll(body: PagedItemTrackingApplicationResultRequestDto){
        let url_ = this.url + "/api/services/app/ItemTrackingApplication/GetAll?";

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

    //CreateTrackingApplication
    createTrackingApplication(CustomerCode: string, CustomerId: number, Total: number, ProductCode: string, ProductDesc: string, PostalCode: string){
        return this.http.post(
            this.url + `/api/services/app/ItemTrackingApplication/CreateTrackingApplication?CustomerCode=${CustomerCode}&CustomerId=${CustomerId}&Total=${Total}&ProductCode=${ProductCode}&ProductDesc=${ProductDesc}&PostalCode=${PostalCode}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }
}
