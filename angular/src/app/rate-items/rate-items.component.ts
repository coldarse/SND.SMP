import { Component, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { RateItemDto } from "@shared/service-proxies/rate-items/model";
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
    }
    else{
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
      .getAll(request)
      .pipe(
        finalize(() => {
          finishedCallback();
          this.isTableLoading = true;
        })
      )
      .subscribe((result: any) => {
        this.rateItems = [];
        this._rateService.getRates().subscribe((rates: any) => {
          this._currencyService.getCurrencies().subscribe((currencies: any) => {
            this.rates = rates.result;
            result.result.items.forEach((element: RateItemDto) => {
              const rateCardName = rates.result.find(
                (x: any) => x.id === element.rateId
              );
              const currency = currencies.result.find(
                (x: any) => x.id === element.currencyId
              );

              let tempRateItem = {
                id: element.id,
                rateId: element.rateId,
                rateCardName: rateCardName.cardName,
                serviceCode: element.serviceCode,
                productCode: element.productCode,
                countryCode: element.countryCode,
                total: element.total,
                fee: element.fee,
                currencyId: element.currencyId,
                currency: currency.abbr,
                paymentMode: element.paymentMode,
              };

              this.rateItems.push(tempRateItem);
            });
            this._rateItemService.getAllRateItemsCount().subscribe((count: any) => {
              this.showPaging(result.result, pageNumber);
              this.rates.unshift({
                id: 0,
                cardName: 'All',
                count: count.result
              });

              this.isTableLoading = false;
            });
          });
        });
      });
  }
}
