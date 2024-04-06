import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { BagDto } from '../../../shared/service-proxies/bags/model';
import { BagService } from '../../../shared/service-proxies/bags/bag.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-create-update-bag',
  templateUrl: './create-update-bag.component.html',
  styleUrls: ['./create-update-bag.component.css']
})
export class CreateUpdateBagComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  bag?: BagDto = {} as BagDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _bagService: BagService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.bag.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.bag.id != undefined){
      this._bagService.update(this.bag).subscribe(
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
      this._bagService.create(this.bag).subscribe(
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
