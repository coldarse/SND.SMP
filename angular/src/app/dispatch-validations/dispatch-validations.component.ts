import { Component, Injector, Input, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { DispatchValidationDto } from "@shared/service-proxies/dispatch-validations/model";
import { DispatchValidationService } from "@shared/service-proxies/dispatch-validations/dispatch-validation.service";
import { CreateUpdateDispatchValidationComponent } from "../dispatch-validations/create-update-dispatchvalidation/create-update-dispatch-validation.component";
import { Router } from "@angular/router";
import { SidebarComponent } from "@app/layout/sidebar.component";

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

  @Input() showPagination: boolean = true;
  @Input() maxItems: number = 10;

  isAdmin = true;
  companyCode = "";

  reloadDispatchValidation: any;

  constructor(
    injector: Injector,
    private router: Router,
    private _dispatchvalidationService: DispatchValidationService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (!this.showPagination) {
      this.startReloadInterval();
    }
    else{
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
        this.showPaging(result.result, pageNumber);
      });
  }
}
