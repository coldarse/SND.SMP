import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedPostalResultRequestDto, PostalDto } from './model';
import { AppConsts } from '@shared/AppConsts';

@Injectable()
export class PostalService {

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

    //Create Postal
    create(body: PostalDto){
        return this.http.post(
            this.url + '/api/services/app/Postal/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Update Postal
    update(body: PostalDto){
        return this.http.put(
            this.url + '/api/services/app/Postal/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Delete Postal
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/Postal/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Postal
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/Postal/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get All Postals
    getAll(body: PagedPostalResultRequestDto){
        let url_ = this.url + "/api/services/app/Postal/GetAll?";

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

    //Upload Excel for Postal
    uploadPostalFile(body: any){
        return this.http.post(
            this.url + '/api/services/app/Postal/UploadPostalFile',
            body,
            {
                headers: new HttpHeaders({
                    "Accept": "text/plain"
                })
            }
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    // Get Postal Drop Down List
    getPostalDDL(){
        return this.http.get(
            this.url + '/api/services/app/Postal/GetPostalDDL',
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Services By Postal
    getServicesByPostal(postalCode: string) {
        return this.http.get(
            this.url + `/api/services/app/Postal/GetServicesByPostal?postalCode=${postalCode}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }

    //Get Products By Postal And Service
    getProductsByPostalAndService(postalCode: string, serviceCode: string) {
        return this.http.get(
            this.url + `/api/services/app/Postal/GetProductsByPostalAndService?postalCode=${postalCode}&serviceCode=${serviceCode}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.handleError),
        )
    }
}
