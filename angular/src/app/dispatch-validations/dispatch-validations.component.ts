import { Component, Injector, Input, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import {
  DispatchValidateDto,
  DispatchValidationDto,
} from "@shared/service-proxies/dispatch-validations/model";
import { DispatchValidationService } from "@shared/service-proxies/dispatch-validations/dispatch-validation.service";
import { CreateUpdateDispatchValidationComponent } from "./create-update-dispatch-validation/create-update-dispatch-validation.component";
import { Router } from "@angular/router";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { DispatchValidationErrorComponent } from "./dipatch-validation-error/dispatch-validation-error.component";
import { QueueService } from "@shared/service-proxies/queues/queue.service";

class PagedDispatchValidationsRequestDto extends PagedRequestDto {
  keyword: string;
  isAdmin: boolean;
  customerCode: string;
  sorting: string;
}

@Component({
  selector: "app-dispatch-validations",
  templateUrl: "./dispatch-validations.component.html",
  styleUrls: ["./dispatch-validations.component.css"],
})
export class DispatchValidationsComponent
  extends PagedListingComponentBase<DispatchValidationDto>
  implements OnInit
{
  keyword = "";
  dispatchvalidations: any[] = [];
  errorValidations: DispatchValidateDto[] = [];

  @Input() showPagination: boolean = true;
  @Input() maxItems: number = 10;

  isAdmin = true;
  companyCode = "";

  reloadDispatchValidation: any;

  constructor(
    injector: Injector,
    private router: Router,
    private _dispatchvalidationService: DispatchValidationService,
    private _chibiService: ChibiService,
    private _queueService: QueueService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (!this.showPagination) {
      this.startReloadInterval();
    } else {
      this.getDataPage(1);
    }
  }

  startReloadInterval() {
    this.getDataPage(1);
    this.reloadDispatchValidation = setInterval(() => {
      this.getDataPage(1);
    }, 30000);
  }

  ngOnDestroy(): void {
    clearInterval(this.reloadDispatchValidation);
  }

  createDispatchValidation() {
    this.showCreateOrEditDispatchValidationDialog();
  }

  editDispatchValidation(entity: DispatchValidationDto) {
    this.showCreateOrEditDispatchValidationDialog(entity);
  }

  private showCreateOrEditDispatchValidationDialog(
    entity?: DispatchValidationDto
  ) {
    let createOrEditDispatchValidationDialog: BsModalRef;
    if (!entity) {
      createOrEditDispatchValidationDialog = this._modalService.show(
        CreateUpdateDispatchValidationComponent,
        {
          class: "modal-lg",
        }
      );
    } else {
      createOrEditDispatchValidationDialog = this._modalService.show(
        CreateUpdateDispatchValidationComponent,
        {
          class: "modal-lg",
          initialState: {
            dispatchvalidation: entity,
          },
        }
      );
    }

    createOrEditDispatchValidationDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  rerouteToModule() {
    this.router.navigate(["/app/dispatch-validations"]);
  }

  protected delete(entity: DispatchValidationDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._dispatchvalidationService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.DispatchValidation." + action);
  }

  selectedErrorDetails(dispatchNo: string) {
    let dispatchValidationErrorDialog: BsModalRef;
    dispatchValidationErrorDialog = this._modalService.show(
      DispatchValidationErrorComponent,
      {
        class: "modal-lg",
        initialState: {
          dispatchNo: dispatchNo
        }
      }
    );

    dispatchValidationErrorDialog.content.onClose.subscribe(() => {
      this._modalService.hide();
    });
  }

  retryDispatchValidation(filepath: string) {
    this._queueService
      .getDispatchValidationUpdateStatusByFilePath(filepath)
      .subscribe((result: any) => {
        if (result.result) this.notify.info(this.l("SavedSuccessfully"));
        else this.notify.error("Failed to Retry");
      });
  }

  postCheck(dispatchNo: string) {
    this.router.navigate(["/app/postchecks"]);
  }

  protected list(
    request: PagedDispatchValidationsRequestDto,
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

    this._dispatchvalidationService
      .getDispatchValidation(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.dispatchvalidations = [];
        result.result.items.forEach((element: DispatchValidationDto) => {
          let tempDispatchValidation = {
            id: element.id,
            customerCode: element.customerCode,
            dateStarted: element.dateStarted,
            dateCompleted: element.dateCompleted,
            dispatchNo: element.dispatchNo,
            filePath: element.filePath,
            isFundLack: element.isFundLack,
            isValid: element.isValid,
            postalCode: element.postalCode,
            serviceCode: element.serviceCode,
            productCode: element.productCode,
            status: element.status,
            tookInSec: element.tookInSec,
            validationProgress: element.validationProgress,
          };

          this.dispatchvalidations.push(tempDispatchValidation);
        });
        if (this.showPagination) this.showPaging(result.result, pageNumber);
      });
  }
}
