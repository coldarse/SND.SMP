import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { CustomerTransactionDto } from '@shared/service-proxies/customertransactions/model';
import { CustomerTransactionService } from '@shared/service-proxies/customertransactions/customertransaction.service';
import { CreateUpdateCustomerTransactionComponent } from './create-update-customertransaction/create-update-customertransaction.component';

class PagedCustomerTransactionsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-customertransactions',
  templateUrl: './customertransactions.component.html',
  styleUrls: ['./customertransactions.component.css']
})

export class CustomerTransactionsComponent extends PagedListingComponentBase<CustomerTransactionDto> {

  keyword = '';
  customerTransactions: any[] = [];

  constructor(
    injector: Injector,
    private _customerTransactionService: CustomerTransactionService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createCustomerTransaction(){
    this.showCreateOrEditCustomerTransactionDialog();
  }

  editCustomerTransaction(entity: CustomerTransactionDto){
    this.showCreateOrEditCustomerTransactionDialog(entity);
  }

  private showCreateOrEditCustomerTransactionDialog(entity?: CustomerTransactionDto){
    let createOrEditCustomerTransactionDialog: BsModalRef;
    if(!entity){
      createOrEditCustomerTransactionDialog = this._modalService.show(
        CreateUpdateCustomerTransactionComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditCustomerTransactionDialog = this._modalService.show(
        CreateUpdateCustomerTransactionComponent,
        {
          class: 'modal-lg',
          initialState: {
            customerTransaction: entity
          },
        }
      );
    }

    createOrEditCustomerTransactionDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: CustomerTransactionDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._customerTransactionService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.CustomerTransaction.' + action);
  }

  protected list(
    request: PagedCustomerTransactionsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._customerTransactionService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.customerTransactions = [];
        result.result.items.forEach((element: CustomerTransactionDto) => {

          let tempCustomerTransaction = {
            wallet: element.wallet,
            customer: element.customer,
            paymentmode: element.paymentMode,
            currency: element.currency,
            transactionType: element.transactionType,
            amount: element.amount,
            referenceNo: element.referenceNo,
            description: element.description,
            transactionDate: element.transactionDate
          }

          this.customerTransactions.push(tempCustomerTransaction);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}

