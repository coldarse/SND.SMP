import { Component, Injector, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { RateWeightBreakDto } from "@shared/service-proxies/rateweightbreaks/model";
import { RateWeightBreakService } from "@shared/service-proxies/rateweightbreaks/rateweightbreak.service";
import { CreateUpdateRateWeightBreakComponent } from "../rateweightbreaks/create-update-rateweightbreak/create-update-rateweightbreak.component";
import { RateService } from "@shared/service-proxies/rates/rate.service";
import { UploadRateWeightBreakComponent } from "./upload-rate-weight-break/upload-rate-weight-break.component";


class PagedRateWeightBreaksRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  selector: "app-rateweightbreaks",
  templateUrl: "./rateweightbreaks.component.html",
  styleUrls: ["./rateweightbreaks.component.css"],
})
export class RateWeightBreaksComponent extends PagedListingComponentBase<RateWeightBreakDto> {
  keyword = "";
  rateweightbreak: any;
  weightbreaks: any[] = [];
  products: any[] = [];
  rates: any[] = [];

  grouped: any;

  selectedRateCard = 0;
  selectedRateCardName = '';

  constructor(
    injector: Injector,
    private _rateweightbreakService: RateWeightBreakService,
    private _rateService: RateService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  createRateWeightBreak() {
    this.showCreateOrEditRateWeightBreakDialog();
  }

  editRateWeightBreak(entity: RateWeightBreakDto) {
    this.showCreateOrEditRateWeightBreakDialog(entity);
  }

  private showCreateOrEditRateWeightBreakDialog(entity?: RateWeightBreakDto) {
    let createOrEditRateWeightBreakDialog: BsModalRef;
    if (!entity) {
      createOrEditRateWeightBreakDialog = this._modalService.show(
        CreateUpdateRateWeightBreakComponent,
        {
          class: "modal-lg",
        }
      );
    } else {
      createOrEditRateWeightBreakDialog = this._modalService.show(
        CreateUpdateRateWeightBreakComponent,
        {
          class: "modal-lg",
          initialState: {
            rateweightbreak: entity,
          },
        }
      );
    }

    createOrEditRateWeightBreakDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  protected delete(entity: RateWeightBreakDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._rateweightbreakService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.RateWeightBreak." + action);
  }

  filter(input: any) {
    if (input.toString() === "0") {
      this.selectedRateCard = 0;
      this.getDataPage(1);
    } else {
      this.selectedRateCard = +input;
      this.selectedRateCardName = this.rates.find(x => x.id === +input).cardName;
      this.getDataPage(1);
    }
  }

  private groupBy(list, keyGetter) {
    const map = new Map();
    list.forEach((item) => {
      const key = keyGetter(item);
      const collection = map.get(key);
      if (!collection) {
        map.set(key, [item]);
      } else {
        collection.push(item);
      }
    });
    return map;
  }
  

  uploadRateWeightBreakExcel(){
    this.showUploadRateWeightBreakDialog();
  }

  private showUploadRateWeightBreakDialog() {
    let uploadRateWeightBreakDialog: BsModalRef;
    uploadRateWeightBreakDialog = this._modalService.show(UploadRateWeightBreakComponent, {
      class: "modal-lg",
    });

    uploadRateWeightBreakDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  protected list(
    request: PagedRateWeightBreaksRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.weightbreaks = [];
    this.products = [];
    this.grouped = undefined;
    this.rateweightbreak = undefined;
    this.isTableLoading = true;

    this._rateService.getRates().subscribe((rates: any) => {
      this.rates = rates.result;

      if (this.selectedRateCard !== 0) {
        this._rateweightbreakService
          .getRateWeightBreakByRate(this.selectedRateCard)
          .pipe(
            finalize(() => {
              finishedCallback();
            })
          )
          .subscribe((rwb: any) => {
            this.rateweightbreak = rwb.result;
            this.weightbreaks = rwb.result.weightBreaks;
            this.products = [
              ...new Set(this.weightbreaks.map((x) => x.productCode)),
            ];

            let exceeds = this.weightbreaks.filter(
              (x) => x.weightBreak === "0.00 - 0.00"
            );

            // -- Filter out Exeeds Row -- //
            this.weightbreaks = this.weightbreaks.filter(
              (x) => x.weightBreak !== "0.00 - 0.00"
            );

            let grouped = this.groupBy(this.weightbreaks, (x: any) => x.weightBreak);

            grouped['Exceeds'] = exceeds;

            this.grouped = grouped;
            this.isTableLoading = false;
          });
      } else {
        this.isTableLoading = false;
      }
    });
  }
}
