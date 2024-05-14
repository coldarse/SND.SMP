import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { catchError, retry, throwError } from "rxjs";
import {
  PagedCustomerTransactionResultRequestDto,
  CustomerTransactionDto,
} from "./model";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";

@Injectable()
export class CustomerTransactionService {
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

  //Create Currency
  create(body: CustomerTransactionDto) {
    return this.http
      .post(
        this.url + "/api/services/app/CustomerTransaction/Create",
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Update Currency
  update(body: CustomerTransactionDto) {
    return this.http
      .put(
        this.url + "/api/services/app/CustomerTransaction/Update",
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Delete Currency
  delete(id: number) {
    return this.http
      .delete(
        this.url +
          `/api/services/app/CustomerTransaction/Delete?Id=${id.toString()}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Currency
  get(id: number) {
    return this.http
      .get(
        this.url + `/api/services/app/CustomerTransaction/Get?Id=${id}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get All Currencies
  getAll(body: PagedCustomerTransactionResultRequestDto) {
    let url_ = this.url + "/api/services/app/CustomerTransaction/GetAll?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    if (body.isAdmin !== undefined)
      url_ += "IsAdmin=" + encodeURIComponent("" + body.isAdmin) + "&";

    if (body.customer !== undefined)
      url_ += "Customer=" + encodeURIComponent("" + body.customer) + "&";

    let count = 10;
    if (body.maxResultCount !== undefined)
      count = body.maxResultCount;
    
    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=${count}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Get All CustomerTransaction without filter
  getCustomerTransaction() {
    return this.http
      .get(
        this.url +
          `/api/services/app/CustomerTransaction/GetCustomerTransactions`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Dashboard Transaction
  getDashboardTransaction(isAdmin: boolean, top: number, customer: string) {
    return this.http
    .get(
      isAdmin ? 
        this.url + `/api/services/app/CustomerTransaction/GetDashboardTransaction?isAdmin=${isAdmin}&top=${top}` :
        this.url + `/api/services/app/CustomerTransaction/GetDashboardTransaction?isAdmin=${isAdmin}&top=${top}&customer=${customer}`,
      this.options_
    )
    .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }
}
