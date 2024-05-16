import { Component, EventEmitter, Injector, Input, Output } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import {
  CustomerPostalDto,
  DetailedCustomerPostalDto,
} from "@shared/service-proxies/customer-postals/model";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { CreateUpdateCustomerPostalComponent } from "./create-update-customer-postal/create-update-customer-postal.component";
import { CustomerDto } from "@shared/service-proxies/customers/model";
import { PostalDDL } from "@shared/service-proxies/postals/model";
import { RateDDL } from "@shared/service-proxies/rates/model";

class PagedCustomerPostalsRequestDto extends PagedRequestDto {
  keyword: string;
  accountNo: number;
}

@Component({
  selector: "app-customer-postals",
  templateUrl: "./customer-postals.component.html",
  styleUrls: ["./customer-postals.component.css"],
})
export class CustomerPostalsComponent extends PagedListingComponentBase<CustomerPostalDto> {
  keyword = "";
  customerpostals: any[] = [];

  postalItems: PostalDDL[] = [];
  rateItems: RateDDL[] = [];

  @Input() selectedCustomer: CustomerDto;

  @Output() getPostalDDLEmit = new EventEmitter<PostalDDL[]>();
  @Output() getRateDDLEmit = new EventEmitter<RateDDL[]>();
  @Output() closeEmit = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _customerpostalService: CustomerPostalService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  createCustomerPostal() {
    this.showCreateOrEditCustomerPostalDialog();
  }

  closePostal(): void {
    this.closeEmit.emit();
  }

  entries(event: any) {
    this.pageSize = event.target.value;
    this.getDataPage(1);
  }

  private showCreateOrEditCustomerPostalDialog() {
    let createOrEditCustomerPostalDialog: BsModalRef;
    createOrEditCustomerPostalDialog = this._modalService.show(
      CreateUpdateCustomerPostalComponent,
      {
        class: "modal-lg",
        initialState: {
          customerpostal: {
            accountNo: this.selectedCustomer.id,
            postal: undefined,
            rate: undefined,
            rateCard: undefined,
            code: undefined,
            createWallet: undefined
          },
          postalItems: this.postalItems,
          rateItems: this.rateItems,
        },
      }
    );

    createOrEditCustomerPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  protected delete(entity: CustomerPostalDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._customerpostalService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.CustomerPostal." + action);
  }

  protected list(
    request: PagedCustomerPostalsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    request.accountNo = this.selectedCustomer.id;
    this._customerpostalService
      .getFullDetailedCustomerPostal(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.postalItems = result.result.postalDDLs;
        this.rateItems = result.result.rateDDLs;
        this.customerpostals = [];
        result.result.pagedResultDto.items.forEach((element: DetailedCustomerPostalDto) => {
          let tempCustomerPostal = {
            id: element.id,
            postal: element.postal,
            rate: element.rate,
            rateCard: element.rateCard,
            accountNo: element.accountNo,
            code: element.code,
          };

          this.customerpostals.push(tempCustomerPostal);
        });
        this.showPaging(result.result.pagedResultDto, pageNumber);
        this.getPostalDDLEmit.emit(this.postalItems);
        this.getRateDDLEmit.emit(this.rateItems);
      });
  }
}
