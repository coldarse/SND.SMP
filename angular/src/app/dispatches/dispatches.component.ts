import { Component, Injector, Input } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import {
  DispatchDto,
  DispatchInfoDto,
  Zip,
} from "@shared/service-proxies/dispatches/model";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { CreateUpdateDispatchComponent } from "../dispatches/create-update-dispatch/create-update-dispatch.component";
import { Router } from "@angular/router";
import { HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { PrePostCheckWeightComponent } from "./pre-post-check-weight/pre-post-check-weight.component";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { GenerateInvoice } from "@shared/service-proxies/invoices/model";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";
import { DatePipe } from "@node_modules/@angular/common";
import { DeDiscountValueComponent } from "./de-discount-value/de-discount-value.component";
import { ApplicationSettingService } from "@shared/service-proxies/applicationsettings/applicationsetting.service";

class PagedDispatchesRequestDto extends PagedRequestDto {
  keyword: string;
  isAdmin: boolean;
  customerCode: string;
  sorting: string;
}

@Component({
  selector: "app-dispatches",
  templateUrl: "./dispatches.component.html",
  styleUrls: ["./dispatches.component.css"],
})
export class DispatchesComponent extends PagedListingComponentBase<DispatchDto> {
  keyword = "";
  dispatches: any[] = [];

  @Input() showHeader: boolean = true;
  @Input() showPagination: boolean = true;
  @Input() maxItems: number = 10;
  @Input() fromCustomerInfo = false;
  @Input() specific_companyCode: string;

  isAdmin = true;
  isDownloadingManifest = false;
  isDownloadingBag = false;
  isUndoingPostCheck = false;
  companyCode = "";

  invoice_info: GenerateInvoice = {} as GenerateInvoice;

  constructor(
    injector: Injector,
    private router: Router,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService,
    private _chibiService: ChibiService,
    private _applicationSettingService: ApplicationSettingService
  ) {
    super(injector);
  }

  createDispatch() {
    this.showCreateOrEditDispatchDialog();
  }

  editDispatch(entity: DispatchDto) {
    this.showCreateOrEditDispatchDialog(entity);
  }

  private showCreateOrEditDispatchDialog(entity?: DispatchDto) {
    let createOrEditDispatchDialog: BsModalRef;
    if (!entity) {
      createOrEditDispatchDialog = this._modalService.show(
        CreateUpdateDispatchComponent,
        {
          class: "modal-lg",
        }
      );
    } else {
      createOrEditDispatchDialog = this._modalService.show(
        CreateUpdateDispatchComponent,
        {
          class: "modal-lg",
          initialState: {
            dispatch: entity,
          },
        }
      );
    }

    createOrEditDispatchDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  protected delete(entity: DispatchDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._dispatchService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.Dispatch." + action);
  }

  rerouteToModule() {
    this.router.navigate(["/app/dispatches"]);
  }

  downloadManifest(dispatchNo: string, status: string) {
    let dmModal: BsModalRef;
    dmModal = this._modalService.show(PrePostCheckWeightComponent, {
      class: "modal-lg",
      initialState: {
        donePostCheck: status == "Post Check",
      },
    });

    dmModal.content.isPreCheckWeight.subscribe((isPreCheckWeight: boolean) => {
      this.isDownloadingManifest = true;
      this._dispatchService
        .downloadManifest(dispatchNo, isPreCheckWeight)
        .pipe(
          finalize(() => {
            this.isDownloadingManifest = false;
          })
        )
        .subscribe((res: HttpResponse<Blob>) => {
          var contentDisposition = res.headers.get("content-disposition");
          var filename = contentDisposition
            .split(";")[1]
            .split("filename")[1]
            .split("=")[1]
            .trim();
          const blob = new Blob([res.body], { type: "application/zip" });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement("a");
          a.href = url;
          a.download = filename;
          a.click();

          // Clean up
          window.URL.revokeObjectURL(url);
          abp.notify.success(this.l("Successfully Downloaded"));
        });
    });
  }

  downloadBag(dispatchNo: string, status: string) {
    let dbModal: BsModalRef;
    dbModal = this._modalService.show(PrePostCheckWeightComponent, {
      class: "modal-lg",
      initialState: {
        donePostCheck: status == "Post Check",
      },
    });

    dbModal.content.isPreCheckWeight.subscribe((isPreCheckWeight: boolean) => {
      this.isDownloadingBag = true;
      this._dispatchService
        .downloadBag(dispatchNo, isPreCheckWeight)
        .pipe(
          finalize(() => {
            this.isDownloadingBag = false;
          })
        )
        .subscribe((res: HttpResponse<Blob>) => {
          var contentDisposition = res.headers.get("content-disposition");
          var filename = contentDisposition
            .split(";")[1]
            .split("filename")[1]
            .split("=")[1]
            .trim();
          const blob = new Blob([res.body], { type: "application/zip" });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement("a");
          a.href = url;
          a.download = filename;
          a.click();

          // Clean up
          window.URL.revokeObjectURL(url);
          abp.notify.success(this.l("Successfully Downloaded"));
        });
    });
  }

  postCheck(dispatchNo: string) {
    this.router.navigate(["/app/postchecks", dispatchNo]);
  }

  undoPostCheck(dispatchNo: string) {
    abp.message.confirm(
      `Undo Post Check for ${dispatchNo}`,
      undefined,
      (result: boolean) => {
        if (result) {
          this.isUndoingPostCheck = true;
          this._dispatchService
            .undoPostCheck(dispatchNo)
            .pipe(
              finalize(() => {
                this.isUndoingPostCheck = false;
              })
            )
            .subscribe(
              () => {
                abp.notify.success(this.l("Successfully Undo Postcheck"));
                this.getDataPage(1);
              },
              (error: HttpErrorResponse) => {
                //Handle error
                let cc: BsModalRef;
                cc = this._modalService.show(ErrorModalComponent, {
                  class: "modal-lg",
                  initialState: {
                    title: "",
                    errorMessage: error.message,
                  },
                });
              }
            );
        }
      }
    );
  }

  entries(event: any) {
    this.pageSize = event.target.value;
    this.getDataPage(1);
  }

  generateCommercialInvoice(companyCode: string, dispatchNo: string) {
    // Get Commercial Invoice Running No
    this._applicationSettingService
      .getValueByName("CommercialInvoiceNo")
      .subscribe((data: any) => {
        let commercial_invoice_no = data.result == "" ? 0 : +data.result;

        // Open Pop-Up for discount assignation
        this._modalService.show(DeDiscountValueComponent, {
          class: "modal-lg",
          initialState: {
            companyCode: companyCode,
            dispatchNo: dispatchNo,
            runningNo: commercial_invoice_no.toString(),
          },
        });
      });
  }

  deleteCommercialInvoice(invoicePath: string) {
    // Call Delete Invoice API based on the invoice file name
    this._chibiService.deleteInvoice(invoicePath).subscribe((data: any) => {
      if (data) abp.notify.success(this.l("SuccessfullyDeleted"));
      else abp.notify.error(this.l("Error Deleting"));
    });
  }

  downloadCommercialInvoiceExcel(dispatchNo: string) {
    // Call Generate and Download Commercial Invoice Excel API based on dispatchNo
    this._modalService.show(DeDiscountValueComponent, {
      class: "modal-lg",
      initialState: {
        dispatchNo: dispatchNo,
        isExcel: true,
      },
    });
  }

  downloadSAManifest(dispatchNo: string) {
    let deDiscountModal: BsModalRef;

    deDiscountModal = this._modalService.show(DeDiscountValueComponent, {
      class: "modal-lg",
      initialState: {
        dispatchNo: dispatchNo,
        isManifest: true,
      },
    });

    deDiscountModal.content.onSubmit.subscribe((data: any[]) => {
      this.isDownloadingManifest = true;
      this._dispatchService
        .downloadSADispatchManifest(
          data[0],
          data[1],
          data[2]
        )
        .pipe(
          finalize(() => {
            this.isDownloadingManifest = false;
          })
        )
        .subscribe((res: HttpResponse<Blob>) => {
          var contentDisposition = res.headers.get("content-disposition");
          var filename = contentDisposition
            .split(";")[1]
            .split("filename")[1]
            .split("=")[1]
            .trim();
          const blob = new Blob([res.body], { type: "application/zip" });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement("a");
          a.href = url;
          a.download = filename;
          a.click();

          // Clean up
          window.URL.revokeObjectURL(url);
          abp.notify.success(this.l("Successfully Downloaded"));
        });
    });
  }

  protected list(
    request: PagedDispatchesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (this.fromCustomerInfo) {
      this.isAdmin = false;
      this.companyCode = this.specific_companyCode;
    } else {
      let admin = this.appSession
        .getShownLoginName()
        .replace(".\\", "")
        .includes("admin");
      this.isAdmin = admin;
      this.companyCode = admin ? "" : this.appSession.getCompanyCode();
    }

    request.keyword = this.keyword;
    request.customerCode = this.companyCode;
    request.isAdmin = this.isAdmin;
    request.maxResultCount = this.pageSize;

    this._dispatchService
      .getDispatchInfoListPaged(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.dispatches = [];
        result.result.items.forEach((element: DispatchInfoDto) => {
          let tempDispatch = {
            customerName: element.customerName,
            customerCode: element.customerCode,
            postalCode: element.postalCode,
            postalDesc: element.postalDesc,
            dispatchDate: element.dispatchDate,
            dispatchNo: element.dispatchNo,
            serviceCode: element.serviceCode,
            serviceDesc: element.serviceDesc,
            productCode: element.productCode,
            productDesc: element.productDesc,
            totalBags: element.totalBags,
            totalWeight: element.totalWeight,
            totalCountry: element.totalCountry,
            status: element.status,
            path: element.path,
            remark: element.remark,
          };

          this.dispatches.push(tempDispatch);
        });
        if (this.showPagination) this.showPaging(result.result, pageNumber);
      });
  }
}
