import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { EWalletTypeDto } from '../../../shared/service-proxies/ewallettypes/model';
import { EWalletTypeService } from '../../../shared/service-proxies/ewallettypes/ewallettype.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-ewallettype',
  templateUrl: './create-update-ewallettype.component.html',
  styleUrls: ['./create-update-ewallettype.component.css']
})
export class CreateUpdateEWalletTypeComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  ewallettype?: EWalletTypeDto = {} as EWalletTypeDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _ewallettypeService: EWalletTypeService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.ewallettype.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.ewallettype.id != undefined){
      this._ewallettypeService.update(this.ewallettype).subscribe(
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
      this._ewallettypeService.create(this.ewallettype).subscribe(
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
