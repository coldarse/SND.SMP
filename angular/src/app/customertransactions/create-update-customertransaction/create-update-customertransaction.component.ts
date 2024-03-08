import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CustomerTransactionDto } from '../../../shared/service-proxies/customertransactions/model';
import { CustomerTransactionService } from '../../../shared/service-proxies/customertransactions/customertransaction.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-customertransaction',
  templateUrl: './create-update-customertransaction.component.html',
  styleUrls: ['./create-update-customertransaction.component.css']
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
    public bsModalRef: BsModalRef
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
        () => {
          this.saving = false;
        }
      );
    }

  }

}
