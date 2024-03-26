import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CustomerPostalDto } from '../../../shared/service-proxies/customer-postals/model';
import { CustomerPostalService } from '../../../shared/service-proxies/customer-postals/customer-postal.service';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { RateDDL } from '@shared/service-proxies/rates/model';
import { PostalDDL } from '@shared/service-proxies/postals/model';

@Component({
  selector: 'app-create-update-customerpostal',
  templateUrl: './create-update-customer-postal.component.html',
  styleUrls: ['./create-update-customer-postal.component.css']
})
export class CreateUpdateCustomerPostalComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  customerpostal?: CustomerPostalDto = {} as CustomerPostalDto;

  postalItems: PostalDDL[];

  rateItems: RateDDL[];

  formInvalid = true;
  
  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _customerpostalService: CustomerPostalService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    // if(this.customerpostal.id != undefined){
    //   this.isCreate = false;
    // }
    this.isCreate = true;
    console.log(this.postalItems)
  }

  selectedPostal(event: any){
    this.customerpostal.postal = event.target.value;
    if(this.customerpostal.rate == undefined || this.customerpostal.rate == 0 || this.customerpostal.postal == '') 
      this.formInvalid = true;
    else 
      this.formInvalid = false;
  }

  selectedRate(event: any){
    this.customerpostal.rate = +event.target.value;
    if(this.customerpostal.postal == undefined || this.customerpostal.postal == '' || this.customerpostal.rate == 0) 
      this.formInvalid = true;
    else 
      this.formInvalid = false;
  }

  save(): void {
    this.saving = true;

    if(this.customerpostal.id != undefined){
      this._customerpostalService.update(this.customerpostal).subscribe(
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
      this._customerpostalService.create(this.customerpostal).subscribe(
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
