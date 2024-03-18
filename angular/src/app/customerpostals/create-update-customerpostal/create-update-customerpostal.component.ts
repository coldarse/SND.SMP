import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CustomerPostalDto } from '../../../shared/service-proxies/customerpostals/model';
import { CustomerPostalService } from '../../../shared/service-proxies/customerpostals/customerpostal.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-customerpostal',
  templateUrl: './create-update-customerpostal.component.html',
  styleUrls: ['./create-update-customerpostal.component.css']
})
export class CreateUpdateCustomerPostalComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  customerpostal?: CustomerPostalDto = {} as CustomerPostalDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _customerpostalService: CustomerPostalService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.customerpostal.id != undefined){
      this.isCreate = false;
    }
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
