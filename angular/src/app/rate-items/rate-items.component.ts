import { Component, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { RateItemDetailDto, RateItemDto } from "@shared/service-proxies/rate-items/model";
import { RateItemService } from "@shared/service-proxies/rate-items/rate-item.service";
import { RateService } from "@shared/service-proxies/rates/rate.service";
import { CurrencyService } from "@shared/service-proxies/currencies/currency.service";
import { UploadRateItemComponent } from "./upload-rate-item/upload-rate-item.component";
// import { CreateUpdateRateItemComponent } from '../rateItems/create-update-rateItem/create-update-rateItem.component'

class PagedRateItemsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  selector: "app-rate-items",
  templateUrl: "./rate-items.component.html",
  styleUrls: ["./rate-items.component.css"],
})
export class RateItemsComponent extends PagedListingComponentBase<RateItemDto> {
  keyword = "";
  rateItems: any[] = [];
  rates: any[] = [];

  constructor(
    injector: Injector,
    private _rateItemService: RateItemService,
    private _rateService: RateService,
    private _currencyService: CurrencyService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  uploadRateItem() {
    this.showUploadRateItemDialog();
  }

  private showUploadRateItemDialog() {
    let uploadRateItemDialog: BsModalRef;
    uploadRateItemDialog = this._modalService.show(UploadRateItemComponent, {
      class: "modal-lg",
    });

    uploadRateItemDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  filter(input: any) {
    if (input.toString() === "0") {
      this.keyword = "";
      this.getDataPage(1);
    } else {
      this.keyword = input.toString();
      this.getDataPage(1);
    }
  }

  protected delete(entity: RateItemDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._rateItemService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.RateItem." + action);
  }

  protected list(
    request: PagedRateItemsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._rateItemService
      .getFullRateItemDetail(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.rateItems = [];
        this.rates = result.result.rates;
        result.result.pagedRateItemResultDto.items.forEach((element: RateItemDetailDto) => {
          let tempRateItem = {
            id: element.id,
            rateId: element.rateId,
            rateCardName: element.rateCardName,
            serviceCode: element.serviceCode,
            productCode: element.productCode,
            countryCode: element.countryCode,
            total: element.total,
            fee: element.fee,
            currencyId: element.currencyId,
            currency: element.currency,
            paymentMode: element.paymentMode,
          };

          this.rateItems.push(tempRateItem);
        });

        this.showPaging(result.result.pagedRateItemResultDto, pageNumber);
      });
  }
}
