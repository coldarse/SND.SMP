import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { Observable, catchError, map, retry, throwError } from "rxjs";
import {
  PagedDispatchResultRequestDto,
  DispatchDto,
  GetPostCheck,
  Zip,
} from "./model";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";

@Injectable()
export class DispatchService {
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

  //Create Dispatch
  create(body: DispatchDto) {
    return this.http
      .post(this.url + "/api/services/app/Dispatch/Create", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Update Dispatch
  update(body: DispatchDto) {
    return this.http
      .put(this.url + "/api/services/app/Dispatch/Update", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Delete Dispatch
  delete(id: number) {
    return this.http
      .delete(
        this.url + `/api/services/app/Dispatch/Delete?Id=${id.toString()}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Dispatch
  get(id: number) {
    return this.http
      .get(this.url + `/api/services/app/Dispatch/Get?Id=${id}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get All Dispatches
  getAll(body: PagedDispatchResultRequestDto) {
    let url_ = this.url + "/api/services/app/Dispatch/GetAll?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    if (body.isAdmin !== undefined)
      url_ += "IsAdmin=" + encodeURIComponent("" + body.isAdmin) + "&";

    if (body.customerCode !== undefined)
      url_ +=
        "CustomerCode=" + encodeURIComponent("" + body.customerCode) + "&";

    let count = 10;
    if (body.maxResultCount !== undefined) count = body.maxResultCount;

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=${count}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Get Post Check
  getPostCheck(dispatchNo: string) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/GetPostCheck?dispatchNo=${dispatchNo}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Undo Post Check
  undoPostCheck(dispatchNo: string) {
    return this.http
      .post(
        this.url +
          `/api/services/app/Dispatch/UndoPostCheck?dispatchNo=${dispatchNo}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Save Post Check
  savePostCheck(getPostCheck: GetPostCheck) {
    return this.http
      .post(
        this.url + `/api/services/app/Dispatch/SavePostCheck`,
        getPostCheck,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Bypass Post Check
  bypassPostCheck(dispatchNo: string, weightGap: number) {
    return this.http
      .post(
        this.url +
          `/api/services/app/Dispatch/ByPassPostCheck?dispatchNo=${dispatchNo}&weightGap=${weightGap}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Upload Post Check
  uploadPostCheck(body: any) {
    return this.http
      .post(this.url + "/api/services/app/Dispatch/UploadPostCheck", body, {
        headers: new HttpHeaders({
          Accept: "text/plain",
        }),
      })
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Upload Post Check For Display
  uploadPostCheckForDisplay(body: any) {
    return this.http
      .post(
        this.url + "/api/services/app/Dispatch/UploadPostCheckForDisplay",
        body,
        {
          headers: new HttpHeaders({
            Accept: "text/plain",
          }),
        }
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Get Dispatch Info
  getDispatchInfoListPaged(body: PagedDispatchResultRequestDto) {
    let url_ =
      this.url + "/api/services/app/Dispatch/GetDispatchInfoListPaged?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    if (body.isAdmin !== undefined)
      url_ += "IsAdmin=" + encodeURIComponent("" + body.isAdmin) + "&";

    if (body.customerCode !== undefined)
      url_ +=
        "CustomerCode=" + encodeURIComponent("" + body.customerCode) + "&";

    let count = 10;
    if (body.maxResultCount !== undefined) count = body.maxResultCount;

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=${count}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  downloadManifest(dispatchNo: string, isPreCheckWeight: boolean) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/DownloadDispatchManifest?dispatchNo=${dispatchNo}&isPreCheckWeight=${isPreCheckWeight}`,
        { responseType: "blob", observe: "response" }
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  downloadBag(dispatchNo: string, isPreCheckWeight: boolean) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/DownloadDispatchBag?dispatchNo=${dispatchNo}&isPreCheckWeight=${isPreCheckWeight}`,
        { responseType: "blob", observe: "response" }
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Dashboard Dispatch
  getDashboardDispatchInfo(isAdmin: boolean, top: number, customer: string) {
    return this.http
      .get(
        isAdmin
          ? this.url +
              `/api/services/app/Dispatch/GetDashboardDispatchInfo?isAdmin=${isAdmin}&top=${top}`
          : this.url +
              `/api/services/app/Dispatch/GetDashboardDispatchInfo?isAdmin=${isAdmin}&top=${top}&customerCode=${customer}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getDispatchesForTracking(filter?: string, countryCode?: string) {
    let completeUrl =
      this.url + "/api/services/app/Dispatch/GetDispatchesForTracking";

    if (filter != undefined && countryCode != undefined)
      completeUrl =
        completeUrl + `?filter=${filter}&countryCode=${countryCode}`;
    else {
      if (countryCode != undefined)
        completeUrl = completeUrl + `?countryCode=${countryCode}`;
      if (filter != undefined) completeUrl = completeUrl + `?filter=${filter}`;
    }

    return this.http
      .get(completeUrl, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getDispatchesByCustomer(customerCode: string) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/GetDispatchesByCustomer?customerCode=${customerCode}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getDispatchesByCustomerMonthCurrency(customerCode: string, monthYear: string, currency: string, custom: boolean) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/GetDispatchesByCustomerMonthCurrency?customerCode=${customerCode}&monthYear=${monthYear}&currency=${currency}&custom=${custom}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getItemsByCurrency(dispatches: string[], generateBy: number) {
    const body = {
      dispatches: dispatches,
      generateBy: generateBy
    }
    return this.http
      .post(
        this.url +
          `/api/services/app/Dispatch/GetItemsByCurrency`,
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Retrieve Commercial Invoice Excel Items
  getCommercialInvoiceExcelItems(dispatchNo: string, discountValue: string) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Dispatch/GetCommercialInvoiceExcelItems?dispatchNo=${dispatchNo}&discountValue=${discountValue}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  private getFilenameFromContentDisposition(
    contentDisposition: string
  ): string {
    if (!contentDisposition) {
      return "";
    }
    const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
    const matches = filenameRegex.exec(contentDisposition);
    if (!matches || !matches[1]) {
      return "";
    }
    return matches[1].replace(/['"]/g, "");
  }


  
}
