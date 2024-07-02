import { Component, Injector, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { RateWeightBreakDto } from "@shared/service-proxies/rate-weight-breaks/model";
import { RateWeightBreakService } from "@shared/service-proxies/rate-weight-breaks/rate-weight-break.service";
import { RateService } from "@shared/service-proxies/rates/rate.service";
import { UploadRateWeightBreakComponent } from "./upload-rate-weight-break/upload-rate-weight-break.component";
import * as XLSX from "xlsx";
import { template } from "lodash-es";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { HttpResponse } from "@angular/common/http";

class PagedRateWeightBreaksRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  selector: "app-rateweightbreaks",
  templateUrl: "./rate-weight-breaks.component.html",
  styleUrls: ["./rate-weight-breaks.component.css"],
})
export class RateWeightBreaksComponent extends PagedListingComponentBase<RateWeightBreakDto> {
  keyword = "";
  rateweightbreak: any;
  weightbreaks: any[] = [];
  products: any[] = [];
  rates: any[] = [];

  selectedRateCard = 0;
  selectedRateCardName = "";

  constructor(
    injector: Injector,
    private _rateweightbreakService: RateWeightBreakService,
    private _rateService: RateService,
    private _modalService: BsModalService,
    private _chibiService: ChibiService
  ) {
    super(injector);
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
      this.selectedRateCardName = this.rates.find(
        (x) => x.id === +input
      ).cardName;
      this.getDataPage(1);
    }
  }

  uploadRateWeightBreakExcel() {
    this.showUploadRateWeightBreakDialog();
  }

  private showUploadRateWeightBreakDialog() {
    let uploadRateWeightBreakDialog: BsModalRef;
    uploadRateWeightBreakDialog = this._modalService.show(
      UploadRateWeightBreakComponent,
      {
        class: "modal-lg",
      }
    );

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
    this.rateweightbreak = undefined;
    this.isTableLoading = true;

    this._rateService.getDERates().subscribe((rates: any) => {
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
            this.weightbreaks = JSON.parse(rwb.result.weightBreaks);
            this.products = rwb.result.products;
            this.isTableLoading = false;
          });
      } else {
        this.isTableLoading = false;
      }
    });
  }

  downloadTemplate() {
    this._chibiService
    .downloadRateWeightBreakTemplate()
    .pipe(
      finalize(() => {
      })
    )
    .subscribe((res: HttpResponse<Blob>) => {
      var contentDisposition = res.headers.get("content-disposition");
      var filename = contentDisposition
        .split(";")[1]
        .split("filename")[1]
        .split("=")[1]
        .trim();
      console.log(filename);
      const blob = new Blob([res.body], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = filename;
      a.click();

      // Clean up
      window.URL.revokeObjectURL(url);
      abp.notify.success(this.l("Successfully Downloaded"));
    });
  }
}
