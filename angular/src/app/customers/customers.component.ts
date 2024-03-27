import { Component, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { CustomerDto } from "@shared/service-proxies/customers/model";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";
import { CreateUpdateCustomerComponent } from "../customers/create-update-customer/create-update-customer.component";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import {
  CustomerPostalDto,
  DetailedCustomerPostalDto,
} from "@shared/service-proxies/customer-postals/model";
import { CreateUpdateCustomerPostalComponent } from "@app/customer-postals/create-update-customer-postal/create-update-customer-postal.component";
import { PostalDDL } from "@shared/service-proxies/postals/model";
import { RateDDL } from "@shared/service-proxies/rates/model";
import { RateService } from "@shared/service-proxies/rates/rate.service";
import { PostalService } from "@shared/service-proxies/postals/postal.service";

class PagedCustomersRequestDto extends PagedRequestDto {
  keyword: string;
}

class PagedCustomerPostalsRequestDto extends PagedRequestDto {
  keyword: string;
  accountNo: number;
}

@Component({
  selector: "app-customers",
  templateUrl: "./customers.component.html",
  styleUrls: ["./customers.component.css"],
})
export class CustomersComponent extends PagedListingComponentBase<CustomerDto> {
  keyword = "";
  postal_keyword = "";
  customers: any[] = [];
  customerpostals: any[] = [];
  selectedCustomer: CustomerDto;
  openPostal = false;
  searchCustomer = true;

  postalItems: PostalDDL[] = [];
  rateItems: RateDDL[] = [];

  constructor(
    injector: Injector,
    private _customerService: CustomerService,
    private _customerpostalService: CustomerPostalService,
    private _rateService: RateService,
    private _postalService: PostalService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  createCustomer() {
    this.showCreateOrEditCustomerDialog();
  }

  editCustomer(entity: CustomerDto) {
    this.showCreateOrEditCustomerDialog(entity);
  }

  createCustomerPostal() {
    this.showCreateOrEditCustomerPostalDialog();
  }

  private showCreateOrEditCustomerPostalDialog(entity?: CustomerPostalDto) {
    let createOrEditCustomerPostalDialog: BsModalRef;
    if (!entity) {
      createOrEditCustomerPostalDialog = this._modalService.show(
        CreateUpdateCustomerPostalComponent,
        {
          class: "modal-lg",
          initialState: {
            customerpostal: {
              accountNo: this.selectedCustomer.id,
              postal: undefined,
              rate: undefined,
            },
            postalItems: this.postalItems,
            rateItems: this.rateItems,
          },
        }
      );
    } else {
      createOrEditCustomerPostalDialog = this._modalService.show(
        CreateUpdateCustomerPostalComponent,
        {
          class: "modal-lg",
          initialState: {
            customerpostal: {
              accountNo: this.selectedCustomer.id,
              postal: undefined,
              rate: undefined,
            },
            postalItems: this.postalItems,
            rateItems: this.rateItems,
          },
        }
      );
    }

    createOrEditCustomerPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  private showCreateOrEditCustomerDialog(entity?: CustomerDto) {
    let createOrEditCustomerDialog: BsModalRef;
    if (!entity) {
      createOrEditCustomerDialog = this._modalService.show(
        CreateUpdateCustomerComponent,
        {
          class: "modal-lg",
        }
      );
    } else {
      createOrEditCustomerDialog = this._modalService.show(
        CreateUpdateCustomerComponent,
        {
          class: "modal-lg",
          initialState: {
            customer: entity,
          },
        }
      );
    }

    createOrEditCustomerDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  postal(entity: CustomerDto): void {
    this.postal_keyword = "";
    this.keyword = entity.code;
    this.selectedCustomer = entity;
    this.openPostal = true;
    this.getDataPage(1);
  }

  closePostal(): void {
    this.keyword = "";
    this.openPostal = false;
    this.selectedCustomer = undefined;
    this.getDataPage(1);
  }

  protected delete(entity: CustomerDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._customerService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.Customer." + action);
  }

  search(searchCustomer: boolean) {
    if (searchCustomer) {
      this.searchCustomer = true;
      this.getDataPage(1);
    } else {
      this.searchCustomer = false;
      this.getDataPage(1);
    }
  }

  entries(event: any) {
    this.pagePostalSize = event.target.value;
    this.getDataPage(1);
  }

  protected deleteCustomerPostal(entity: CustomerPostalDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._customerpostalService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  protected list(
    request: any,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (this.selectedCustomer == undefined) {
      let customer_request: PagedCustomersRequestDto;
      customer_request = request;
      customer_request.keyword = this.keyword;
      this._customerService
        .getAll(request)
        .pipe(
          finalize(() => {
            finishedCallback();
          })
        )
        .subscribe((result: any) => {
          this.customers = [];
          result.result.items.forEach((element: CustomerDto) => {
            let tempCustomer = {
              id: element.id,
              code: element.code,
              companyName: element.companyName,
              emailAddress: element.emailAddress,
              password: element.password,
              addressLine1: element.addressLine1,
              addressLine2: element.addressLine2,
              city: element.city,
              state: element.state,
              country: element.country,
              phoneNumber: element.phoneNumber,
              registrationNo: element.registrationNo,
              emailAddress2: element.emailAddress2,
              emailAddress3: element.emailAddress3,
              isActive: element.isActive,
            };

            this.customers.push(tempCustomer);
          });
          this.showPaging(result.result, pageNumber);
        });
    } else {
      let postal_request: PagedCustomerPostalsRequestDto;
      postal_request = request;
      postal_request.keyword = this.postal_keyword;
      postal_request.accountNo = this.selectedCustomer.id;
      this.customerpostals = [];
      this._customerpostalService
        .getAll(request)
        .pipe(
          finalize(() => {
            finishedCallback();
          })
        )
        .subscribe((result: any) => {
          this._rateService.getRateDDL().subscribe((rateResult: any) => {
            this._postalService
              .getPostalDDL()
              .subscribe((postalResult: any) => {
                this.postalItems = postalResult.result;
                this.rateItems = rateResult.result;
                this.customerpostals = [];
                result.result.items.forEach(
                  (element: DetailedCustomerPostalDto) => {
                    let tempCustomerPostal = {
                      id: element.id,
                      postal: element.postal,
                      rate: element.rate,
                      rateCard: element.rateCard,
                      accountNo: element.accountNo,
                      code: element.code,
                    };

                    this.customerpostals.push(tempCustomerPostal);
                  }
                );

                this.showPostalPaging(result.result, pageNumber);
              });
          });

          
        });

      let customer_request: PagedCustomersRequestDto;
      customer_request = request;
      customer_request.keyword = this.keyword;
      this._customerService
        .getAll(request)
        .pipe(
          finalize(() => {
            finishedCallback();
          })
        )
        .subscribe((result: any) => {
          this.customers = [];
          result.result.items.forEach((element: CustomerDto) => {
            let tempCustomer = {
              id: element.id,
              code: element.code,
              companyName: element.companyName,
              emailAddress: element.emailAddress,
              password: element.password,
              addressLine1: element.addressLine1,
              addressLine2: element.addressLine2,
              city: element.city,
              state: element.state,
              country: element.country,
              phoneNumber: element.phoneNumber,
              registrationNo: element.registrationNo,
              emailAddress2: element.emailAddress2,
              emailAddress3: element.emailAddress3,
              isActive: element.isActive,
            };

            this.customers.push(tempCustomer);
          });
          this.showPaging(result.result, pageNumber);
        });
    }
  }

  totalPostalPages = 1;
  totalPostalItems = 0;
  pagePostalNumber = 1;
  pagePostalSize = 10;

  private showPostalPaging(result: PagedResultDto, pageNumber: number): void {
    this.totalPostalPages = ((result.totalCount - (result.totalCount % this.pageSize)) / this.pageSize) + 1;

    this.totalPostalItems = result.totalCount;
    this.pagePostalNumber = pageNumber;
}
}
