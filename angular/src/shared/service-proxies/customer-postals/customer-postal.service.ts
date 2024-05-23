import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { catchError, retry, throwError } from "rxjs";
import {
  PagedCustomerPostalResultRequestDto,
  CustomerPostalDto,
} from "./model";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";

@Injectable()
export class CustomerPostalService {
  url = "";
  options_: any;

  constructor(private http: HttpClient, private errorMessage: ErrorMessage){
    this.url = AppConsts.remoteServiceBaseUrl;
    this.options_ = {
      headers: new HttpHeaders({
        "Content-Type": "application/json-patch+json",
        Accept: "text/plain",
      }),
    };
  }

  // private handleError(error: HttpErrorResponse) {
  //   if (error.error instanceof ErrorEvent) {
  //     console.error("An error occurred:", error.error.message);
  //   } else {
  //     console.error(
  //       `Backend returned code ${error.status}, ` + `body was: ${error.error}`
  //     );
  //   }
  //   return throwError(() => new Error(error.error.message));
  // }

  //Create CustomerPostal
  create(body: CustomerPostalDto) {
    return this.http
      .post(
        this.url + "/api/services/app/CustomerPostal/Create",
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Update CustomerPostal
  update(body: CustomerPostalDto) {
    return this.http
      .put(
        this.url + "/api/services/app/CustomerPostal/Update",
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Delete CustomerPostal
  delete(id: number) {
    return this.http
      .delete(
        this.url +
          `/api/services/app/CustomerPostal/Delete?Id=${id.toString()}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get CustomerPostal
  get(id: number) {
    return this.http
      .get(
        this.url + `/api/services/app/CustomerPostal/Get?Id=${id}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get All CustomerPostals
  getAll(body: PagedCustomerPostalResultRequestDto) {
    let url_ = this.url + "/api/services/app/CustomerPostal/GetAll?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    if (body.accountNo !== undefined)
      url_ += "AccountNo=" + encodeURIComponent("" + body.accountNo) + "&";

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=10`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Customer Postals By AccountNo
  getCustomerPostalsByAccountNo(accountNo: number) {
    return this.http
      .get(
        this.url +
          `/api/services/app/CustomerPostal/GetCustomerPostalsByAccountNo?accountNo=${accountNo}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Is Currency Wallet Exists 
  isCurrencyWalletExist(rate: number, accNo: number){
    return this.http
    .post(
      this.url + `/api/services/app/CustomerPostal/IsCurrencyWalletExist?rate=${rate}&accNo=${accNo}`,
      this.options_
    )
    .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Get Full Customer Postal Details
  getFullDetailedCustomerPostal(body: PagedCustomerPostalResultRequestDto) {
    let url_ =
      this.url +
      "/api/services/app/CustomerPostal/GetFullDetailedCustomerPostal?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    if (body.accountNo !== undefined)
      url_ += "AccountNo=" + encodeURIComponent("" + body.accountNo) + "&";

    let count = 10;
    if (body.maxResultCount !== undefined) 
      count = body.maxResultCount;

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=${count}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Grouped Customer Postals 
  getGroupedCustomerPostal() {
    return this.http
      .get(
        this.url +
          `/api/services/app/CustomerPostal/GetGroupedCustomerPostal`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }
}
