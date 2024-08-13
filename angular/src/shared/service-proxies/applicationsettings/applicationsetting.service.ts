import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedApplicationSettingResultRequestDto, ApplicationSettingDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ErrorMessage } from '../error-handling';

@Injectable()
export class ApplicationSettingService {

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

    //Create ApplicationSetting
    create(body: ApplicationSettingDto){
        return this.http.post(
            this.url + '/api/services/app/ApplicationSetting/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Update ApplicationSetting
    update(body: ApplicationSettingDto){
        return this.http.put(
            this.url + '/api/services/app/ApplicationSetting/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Delete ApplicationSetting
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/ApplicationSetting/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get ApplicationSetting
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/ApplicationSetting/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All ApplicationSettings
    getAll(body: PagedApplicationSettingResultRequestDto){
        let url_ = this.url + "/api/services/app/ApplicationSetting/GetAll?";

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

    getValueByName(name: string) {
        return this.http.get(
            this.url + `/api/services/app/ApplicationSetting/GetValueByName?name=${name}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    updateValueByName(name: string, value: string) {
        return this.http.post(
            this.url + `/api/services/app/ApplicationSetting/UpdateValueByName?name=${name}&value=${value}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }
}
