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
  Stage4Update,
  Zip,
} from "@shared/service-proxies/dispatches/model";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { CreateUpdateDispatchComponent } from "../dispatches/create-update-dispatch/create-update-dispatch.component";
import { Router } from "@angular/router";
import { HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { PrePostCheckWeightComponent } from "./pre-post-check-weight/pre-post-check-weight.component";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { Stage4AirportComponent } from "./stage-4-airport/stage-4-airport.component";
import { AirportService } from "@shared/service-proxies/airports/airport.service";
import { AirportDto } from "@shared/service-proxies/airports/model";

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
  airports: AirportDto[] = [];

  undoingPostCheck = false;
  undoingStage3 = false;
  undoingStage4 = false;

  updatingStage3 = false;
  updatingStage4 = false;

  constructor(
    injector: Injector,
    private router: Router,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService,
    private _airportService: AirportService,
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
    abp.message.confirm(`Undo Post Check for ${dispatchNo}`, undefined, (result: boolean) => {
      if (result) {
        this.undoingPostCheck = true;
        this._dispatchService.undoPostCheck(dispatchNo)
        .pipe(
          finalize(() => {
            this.undoingPostCheck = false;
          })
        )
        .subscribe(() => {
          abp.notify.success(this.l("Successfully Undoid Post Check"));
          this.getDataPage(1);
        },
        (error: HttpErrorResponse) => {
          //Handle error
          let cc: BsModalRef;
          cc = this._modalService.show(
            ErrorModalComponent,
            {
              class: 'modal-lg',
              initialState: {
                title: "",
                errorMessage: error.message,
              },
            }
          )
        });
      }
    });
  }

  stage3Update(dispatchNo: string) {
    this.updatingStage3 = true;
    this._dispatchService.stage3Update(dispatchNo)
    .pipe(
      finalize(() => {
        this.updatingStage3 = false;
      })
    )
    .subscribe(() => {
      abp.notify.success(this.l("Successfully Updated Stage 3"));
      this.getDataPage(1);
    },
    (error: HttpErrorResponse) => {
      //Handle error
      let cc: BsModalRef;
      cc = this._modalService.show(
        ErrorModalComponent,
        {
          class: 'modal-lg',
          initialState: {
            title: "",
            errorMessage: error.message,
          },
        }
      )
    });
  }

  undoStage3(dispatchNo: string) {
    abp.message.confirm(`Undo Stage 3 for ${dispatchNo}`, undefined, (result: boolean) => {
      if (result) {
        this.undoingStage3 = true;
        this._dispatchService.stage3Undo(dispatchNo)
        .pipe(
          finalize(() => {
            this.undoingStage3 = false;
          })
        )
        .subscribe(() => {
          abp.notify.success(this.l("Successfully Undid Stage 3"));
          this.getDataPage(1);
        },
        (error: HttpErrorResponse) => {
          //Handle error
          let cc: BsModalRef;
          cc = this._modalService.show(
            ErrorModalComponent,
            {
              class: 'modal-lg',
              initialState: {
                title: "",
                errorMessage: error.message,
              },
            }
          )
        });
      }
    });
  }

  stage4Update(dispatchNo: string, countries: string[]) {
    let stgModal: BsModalRef;
    stgModal = this._modalService.show(Stage4AirportComponent, {
      class: "modal-lg",
      initialState: {
        airports: this.airports,
        countries: countries
      },
    });

    stgModal.content.closeModal.subscribe((countriesWithAirports: any[]) => {
      let stage4Update: Stage4Update = {
        dispatchNo: dispatchNo,
        countryWithAirports: countriesWithAirports
      }
      this.updatingStage4 = true;
      this._dispatchService.stage4Update(stage4Update)
      .pipe(
        finalize(() => {
          this.updatingStage4 = false;
        })
      )
      .subscribe(() => {
        abp.notify.success(this.l("Successfully Updated Stage 4"));
        this.getDataPage(1);
      },
      (error: HttpErrorResponse) => {
        stgModal.hide();
        //Handle error
        let cc: BsModalRef;
        cc = this._modalService.show(
          ErrorModalComponent,
          {
            class: 'modal-lg',
            initialState: {
              title: "",
              errorMessage: error.message,
            },
          }
        )
      });
    });


  }

  undoStage4(dispatchNo: string) {
    abp.message.confirm(`Undo Stage 4 for ${dispatchNo}`, undefined, (result: boolean) => {
      if (result) {
        this.undoingStage4 = true;
        this._dispatchService.stage4Undo(dispatchNo)
        .pipe(
          finalize(() => {
            this.undoingStage4 = false;
          })
        )
        .subscribe(() => {
          abp.notify.success(this.l("Successfully Undid Stage 4"));
          this.getDataPage(1);
        },
        (error: HttpErrorResponse) => {
          //Handle error
          let cc: BsModalRef;
          cc = this._modalService.show(
            ErrorModalComponent,
            {
              class: 'modal-lg',
              initialState: {
                title: "",
                errorMessage: error.message,
              },
            }
          )
        });
      }
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
            countries: element.countries,
          };

          this.dispatches.push(tempDispatch);
        });
        if (this.showPagination) this.showPaging(result.result, pageNumber);

        this._airportService.getAllAirports().subscribe((res:any) => {
          this.airports = res.result;
        })
      });
  }
}
