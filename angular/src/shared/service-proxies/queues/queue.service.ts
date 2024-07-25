import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, retry, throwError } from 'rxjs';
import { PagedQueueResultRequestDto, QueueDto } from './model';
import { AppConsts } from '@shared/AppConsts';
import { ErrorMessage } from '../error-handling';

@Injectable()
export class QueueService {

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

    //Create Queue
    create(body: QueueDto){
        return this.http.post(
            this.url + '/api/services/app/Queue/Create',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Update Queue
    update(body: QueueDto){
        return this.http.put(
            this.url + '/api/services/app/Queue/Update',
            body,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Delete Queue
    delete(id: number){
        return this.http.delete(
            this.url + `/api/services/app/Queue/Delete?Id=${id.toString()}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get Queue
    get(id: number){
        return this.http.get(
            this.url + `/api/services/app/Queue/Get?Id=${id}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    //Get All Queues
    getAll(body: PagedQueueResultRequestDto){
        let url_ = this.url + "/api/services/app/Queue/GetAll?";

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

    getDispatchValidationUpdateStatusByFilePath(filepath: string){
        return this.http.get(
            this.url + `/api/services/app/Queue/GetDispatchValidationUpdateStatusByFilePath?filePath=${filepath}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }

    restartQueue(queueId: number){
        return this.http.get(
            this.url + `/api/services/app/Queue/RestartQueue?queueId=${queueId}`,
            this.options_
        ).pipe(
            retry(1),
            catchError(this.errorMessage.HandleErrorResponse),
        )
    }
}
