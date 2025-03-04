import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { catchError, retry, throwError } from "rxjs";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";
import { DispatchValidateDto } from "../dispatch-validations/model";
import { DispatchInfo, DispatchTracking } from "../dispatches/model";
import { GenerateInvoice } from "../invoices/model";

@Injectable()
export class ChibiService {
  url = "";
  options_: any;

  constructor(private http: HttpClient, private errorMessage: ErrorMessage) {
    this.url = AppConsts.remoteServiceBaseUrl;
    this.options_ = {
      headers: new HttpHeaders({
        "Content-Type": "application/json-patch+json",
        Accept: "text/plain",
      }),
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

  //Upload Excel for Pre Check
  uploadPreCheckFile(body: any) {
    return this.http
      .post(this.url + "/api/services/app/Chibi/PreCheckUpload", body, {
        headers: new HttpHeaders({
          Accept: "text/plain",
        }),
      })
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Upload Excel for Pre Check Retry
  uploadRetryPreCheckFile(body: any) {
    return this.http
      .post(this.url + "/api/services/app/Chibi/PreCheckRetryUpload", body, {
        headers: new HttpHeaders({
          Accept: "text/plain",
        }),
      })
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getDispatchValidationError(dispatchNo: string) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Chibi/GetDispatchValidationError?dispatchNo=${dispatchNo}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  downloadRateWeightBreakTemplate() {
    return this.http
      .get(this.url + `/api/services/app/Chibi/GetRateWeightBreakTemplate`, {
        responseType: "blob",
        observe: "response",
      })
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  downloadItemTrackingIds(applicationId: number) {
    return this.http
      .get(this.url + `/api/services/app/Chibi/GetItemTrackingIds?applicationId=${applicationId}`, {
        responseType: "blob",
        observe: "response",
      })
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  deleteDispatch(path: string, dispatchNo: string) {
    return this.http
      .delete(
        this.url + `/api/services/app/Chibi/DeleteDispatch?path=${path}&dispatchNo=${dispatchNo}`,
        this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  deleteInvoice(path: string) {
    return this.http
      .delete(
        this.url + `/api/services/app/Chibi/DeleteInvoice?path=${path}`,
        this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  createTrackingFileForDispatches(body: DispatchInfo[]) {
    return this.http
      .post(
        this.url + `/api/services/app/Chibi/CreateTrackingFileForDispatches`,
        body,
        this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  createInvoiceQueue(body: GenerateInvoice) {
    return this.http
      .post(
        this.url + `/api/services/app/Chibi/CreateInvoiceQueue`,
        body,
        this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }
}
