import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CustomerDto } from '../../../shared/service-proxies/customers/model';
import { CustomerService } from '../../../shared/service-proxies/customers/customer.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-customer',
  templateUrl: './create-update-customer.component.html',
  styleUrls: ['./create-update-customer.component.css']
})
export class CreateUpdateCustomerComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  customer?: CustomerDto = {} as CustomerDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _customerService: CustomerService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.customer.id != undefined){
      this.isCreate = false;
    }

    this.customer.addressLine1 = '-';
    this.customer.addressLine2 = '-';
    this.customer.city = '-';
    this.customer.state = '-';
    this.customer.country = '-';
    this.customer.phoneNumber = '-';
    this.customer.registrationNo = '-';
    this.customer.emailAddress2 = '-';
    this.customer.emailAddress3 = '-';
    this.customer.isActive = true;
  }

  save(): void {
    this.saving = true;

    if(this.customer.id != undefined){
      this._customerService.update(this.customer).subscribe(
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
      this._customerService.create(this.customer).subscribe(
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
