import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { CustomerDto } from '@shared/service-proxies/customers/model'
import { CustomerService } from '@shared/service-proxies/customers/customer.service'
import { CreateUpdateCustomerComponent } from '../customers/create-update-customer/create-update-customer.component'

class PagedCustomersRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.css']
})
export class CustomersComponent extends PagedListingComponentBase<CustomerDto> {

  keyword = '';
  customers: any[] = [];

  constructor(
    injector: Injector,
    private _customerService: CustomerService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createCustomer(){
    this.showCreateOrEditCustomerDialog();
  }

  editCustomer(entity: CustomerDto){
    this.showCreateOrEditCustomerDialog(entity);
  }

  private showCreateOrEditCustomerDialog(entity?: CustomerDto){
    let createOrEditCustomerDialog: BsModalRef;
    if(!entity){
      createOrEditCustomerDialog = this._modalService.show(
        CreateUpdateCustomerComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditCustomerDialog = this._modalService.show(
        CreateUpdateCustomerComponent,
        {
          class: 'modal-lg',
          initialState: {
            customer: entity
          },
        }
      );
    }

    createOrEditCustomerDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: CustomerDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._customerService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Customer.' + action);
  }

  protected list(
    request: PagedCustomersRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._customerService
    .getAll(
      request
    ).pipe(
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
          }

          this.customers.push(tempCustomer);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
