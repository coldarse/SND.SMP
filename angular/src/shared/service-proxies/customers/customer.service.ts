import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { catchError, retry, throwError } from "rxjs";
import { PagedCustomerResultRequestDto, CustomerDto } from "./model";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";
import { ChangePasswordDto } from "../service-proxies";

@Injectable()
export class CustomerService {
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

  //Create Customer
  create(body: CustomerDto) {
    return this.http
      .post(this.url + "/api/services/app/Customer/Create", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Update Customer
  update(body: CustomerDto) {
    return this.http
      .put(this.url + "/api/services/app/Customer/Update", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Delete Customer
  delete(id: number) {
    return this.http
      .delete(
        this.url + `/api/services/app/Customer/Delete?Id=${id.toString()}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Customer
  get(id: number) {
    return this.http
      .get(this.url + `/api/services/app/Customer/Get?Id=${id}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get All Customers
  getAll(body: PagedCustomerResultRequestDto) {
    let url_ = this.url + "/api/services/app/Customer/GetAll?";

    if (body.keyword === null)
      throw new Error("The parameter 'keyword' cannot be null.");
    else if (body.keyword !== undefined)
      url_ += "Keyword=" + encodeURIComponent("" + body.keyword) + "&";

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=10`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Company Name
  getCompanyNameAndCode(email: string) {
    return this.http
      .get(
        this.url +
          `/api/services/app/Customer/GetCompanyNameAndCode?email=${email}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  // Get All Customers without filter
  getAllCustomers() {
    return this.http
      .get(
        this.url + `/api/services/app/Customer/GetAllCustomers`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  changePassword(body: ChangePasswordDto) {
    return this.http
      .post(
        this.url + "/api/services/app/Customer/ChangePassword",
        body,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getCustomers() {
    return this.http
      .get(this.url + `/api/services/app/Customer/GetCustomers`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }
}
