import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from "@angular/common/http";
import { catchError, retry, throwError } from "rxjs";
import {
  PagedItemResultRequestDto,
  ItemDto,
  PagedAPIItemIdResultDto,
  GetAPIItemIdDetail,
} from "./model";
import { AppConsts } from "@shared/AppConsts";
import { ErrorMessage } from "../error-handling";

@Injectable()
export class ItemService {
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

  //Create Item
  create(body: ItemDto) {
    return this.http
      .post(this.url + "/api/services/app/Item/Create", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Update Item
  update(body: ItemDto) {
    return this.http
      .put(this.url + "/api/services/app/Item/Update", body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Delete Item
  delete(id: number) {
    return this.http
      .delete(
        this.url + `/api/services/app/Item/Delete?Id=${id.toString()}`,
        this.options_
      )
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get Item
  get(id: number) {
    return this.http
      .get(this.url + `/api/services/app/Item/Get?Id=${id}`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  //Get All Items
  getAll(body: PagedItemResultRequestDto) {
    let url_ = this.url + "/api/services/app/Item/GetAll?";

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

  getAPIItemIdDashboard(body: PagedAPIItemIdResultDto) {
    let url_ =
      this.url +
      `/api/services/app/Item/GetAPIItemIdDashboard?month=${body.month}&year=${body.year}&`;

    if (body.skipCount !== undefined)
      url_ += "SkipCount=" + encodeURIComponent("" + body.skipCount) + "&";

    url_ = url_.replace(/[?&]$/, "");

    return this.http
      .get(url_ + `&MaxResultCount=10`, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }

  getAPIItemIdByDistinctAndDay(body: GetAPIItemIdDetail) {
    return this.http
      .post(this.url + `/api/services/app/Item/GetAPIItemIdByDistinctAndDay`, body, this.options_)
      .pipe(retry(1), catchError(this.errorMessage.HandleErrorResponse));
  }
}
