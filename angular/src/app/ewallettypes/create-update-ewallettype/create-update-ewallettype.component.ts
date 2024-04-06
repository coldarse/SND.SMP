import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { EWalletTypeDto } from '../../../shared/service-proxies/ewallettypes/model';
import { EWalletTypeService } from '../../../shared/service-proxies/ewallettypes/ewallettype.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

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
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
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
      this._ewallettypeService.create(this.ewallettype).subscribe(
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
