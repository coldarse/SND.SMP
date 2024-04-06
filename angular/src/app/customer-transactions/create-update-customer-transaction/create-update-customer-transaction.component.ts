import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CustomerTransactionDto } from '../../../shared/service-proxies/customer-transactions/model';
import { CustomerTransactionService } from '../../../shared/service-proxies/customer-transactions/customer-transaction.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-customer-transaction',
  templateUrl: './create-update-customer-transaction.component.html',
  styleUrls: ['./create-update-customer-transaction.component.css']
})
export class CreateUpdateCustomerTransactionComponent extends AppComponentBase
implements OnInit
{

  saving = false;
  isCreate = true;
  customerTransaction?: CustomerTransactionDto = {} as CustomerTransactionDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _customerTransacationService: CustomerTransactionService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.customerTransaction.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.customerTransaction.id != undefined){
      this._customerTransacationService.update(this.customerTransaction).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
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
        },
        () => {
          this.saving = false;
        }
      );
    }
    else{
      this._customerTransacationService.create(this.customerTransaction).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
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
        },
        () => {
          this.saving = false;
        }
      );
    }

  }

}
