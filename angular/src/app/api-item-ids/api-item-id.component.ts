import { Component, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { ItemService } from "@shared/service-proxies/items/item.service";
import { APIItemIdByDistinctAndDay, APIItemIdDashboard } from "@shared/service-proxies/items/model";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";

class PagedApiItemIdRequestDto extends PagedRequestDto {
  month: string;
  year: string;
}

@Component({
  selector: "app-api-item-ids",
  templateUrl: "./api-item-id.component.html",
  styleUrls: ["./api-item-id.component.css"],
})
export class APIItemIdComponent extends PagedListingComponentBase<APIItemIdDashboard> {
  protected delete(entity: APIItemIdDashboard): void {
    throw new Error("Method not implemented.");
  }

  month = "";
  year = "";
  apiItemIds: any[] = [];
  distinctedApiItemIds: APIItemIdByDistinctAndDay[] = [];

  months = [];
  years = [];

  loadFinished = false;

  constructor(
    injector: Injector,
    private _itemService: ItemService,
    private _modalService: BsModalService
  ) {
    super(injector);

    const currentDate = new Date();
    this.month = currentDate.toLocaleString("default", { month: "long" });
    this.year = currentDate.getFullYear().toString();

    this.months = this.getMonthsOfYear();
    this.years = this.getRecentYears();
  }

  selectedMonth(event: any) {
    this.month = event.target.value;
  }

  selectedYear(event: any) {
    this.year = event.target.value;
  }

  getMonthsOfYear(): string[] {
    const months: string[] = [];
    const date = new Date();

    for (let month = 0; month < 12; month++) {
      date.setMonth(month);
      const monthName = date.toLocaleString("default", { month: "long" });
      months.push(monthName);
    }

    return months;
  }

  getRecentYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years: number[] = [];

    for (let year = currentYear; year >= currentYear - 10; year--) {
      years.push(year);
    }

    return years;
  }

  details(apiItemId: any) {
    this.loadFinished = false;
    apiItemId.isLoading = true;
    const body = {
        customerCode: apiItemId.customerCode,
        postalCode: apiItemId.postalCode,
        productCode: apiItemId.productCode,
        serviceCode: apiItemId.serviceCode,
        month: this.month,
        year: this.year,
    }

    this._itemService
    .getAPIItemIdByDistinctAndDay(body)
    .pipe(finalize(() => {
        this.loadFinished = true;
        apiItemId.isLoading = false;
      }))
    .subscribe((element: any) => {
        this.distinctedApiItemIds = element.result;
    });
  }

  protected list(
    request: PagedApiItemIdRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.month = this.month;
    request.year = this.year;
    this._itemService
      .getAPIItemIdDashboard(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.apiItemIds = [];
        result.result.items.forEach((element: APIItemIdDashboard) => {
          let tempApiItemId = {
            customerCode: element.customerCode,
            postalCode: element.postalCode,
            serviceCode: element.serviceCode,
            productCode: element.productCode,
            postalDesc: element.postalDesc,
            serviceDesc: element.serviceDesc,
            productDesc: element.productDesc,
            totalItems: element.totalItems,
            dateLastReceived: element.dateLastReceived,
            isLoading: false,
          };

          this.apiItemIds.push(tempApiItemId);
        });
        this.showPaging(result.result, pageNumber);
      });
  }
}
