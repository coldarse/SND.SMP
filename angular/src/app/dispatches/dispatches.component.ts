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
import { HttpResponse } from "@angular/common/http";
import { PrePostCheckWeightComponent } from "./pre-post-check-weight/pre-post-check-weight.component";

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

  isAdmin = true;
  isDownloadingManifest = false;
  isDownloadingBag = false;
  companyCode = "";

  constructor(
    injector: Injector,
    private router: Router,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService
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
          console.log(filename);
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
          console.log(filename);
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
    this._dispatchService.undoPostCheck(dispatchNo).subscribe(() => {
      abp.notify.success(this.l("Successfully Undo Postcheck"));
      this.getDataPage(1);
    });
  }

  protected list(
    request: PagedDispatchesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    let admin = this.appSession
      .getShownLoginName()
      .replace(".\\", "")
      .includes("admin");
    this.isAdmin = admin;
    this.companyCode = admin ? "" : this.appSession.getCompanyCode();

    request.keyword = this.keyword;
    request.customerCode = this.companyCode;
    request.isAdmin = this.isAdmin;
    request.maxResultCount = this.maxItems;

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
          };

          this.dispatches.push(tempDispatch);
        });
        if (this.showPagination) this.showPaging(result.result, pageNumber);
      });
  }
}
