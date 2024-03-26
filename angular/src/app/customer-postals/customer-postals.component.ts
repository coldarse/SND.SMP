import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { CustomerPostalDto } from '@shared/service-proxies/customer-postals/model'
import { CustomerPostalService } from '@shared/service-proxies/customer-postals/customerpostal.service'
import { CreateUpdateCustomerPostalComponent } from './create-update-customer-postal/create-update-customer-postal.component'

class PagedCustomerPostalsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-customerpostals',
  templateUrl: './customer-postals.component.html',
  styleUrls: ['./customer-postals.component.css']
})
export class CustomerPostalsComponent extends PagedListingComponentBase<CustomerPostalDto> {

  keyword = '';
  customerpostals: any[] = [];

  constructor(
    injector: Injector,
    private _customerpostalService: CustomerPostalService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createCustomerPostal(){
    this.showCreateOrEditCustomerPostalDialog();
  }

  editCustomerPostal(entity: CustomerPostalDto){
    this.showCreateOrEditCustomerPostalDialog(entity);
  }

  private showCreateOrEditCustomerPostalDialog(entity?: CustomerPostalDto){
    let createOrEditCustomerPostalDialog: BsModalRef;
    if(!entity){
      createOrEditCustomerPostalDialog = this._modalService.show(
        CreateUpdateCustomerPostalComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditCustomerPostalDialog = this._modalService.show(
        CreateUpdateCustomerPostalComponent,
        {
          class: 'modal-lg',
          initialState: {
            customerpostal: entity
          },
        }
      );
    }

    createOrEditCustomerPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: CustomerPostalDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._customerpostalService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.CustomerPostal.' + action);
  }

  protected list(
    request: PagedCustomerPostalsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._customerpostalService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.customerpostals = [];
        result.result.items.forEach((element: CustomerPostalDto) => {

          let tempCustomerPostal = {
            id: element.id,
            postal: element.postal,
            rate: element.rate,
            accountNo: element.accountNo,
          }

          this.customerpostals.push(tempCustomerPostal);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
