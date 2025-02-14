import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { DispatchDto } from '../../../shared/service-proxies/dispatches/model';
import { DispatchService } from '../../../shared/service-proxies/dispatches/dispatch.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-dispatch',
  templateUrl: './create-update-dispatch.component.html',
  styleUrls: ['./create-update-dispatch.component.css']
})
export class CreateUpdateDispatchComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  dispatch?: DispatchDto = {} as DispatchDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _dispatchService: DispatchService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.dispatch.id != undefined){
      this.isCreate = false;
      this.dispatch.dispatchDate                     = this.dispatch.dispatchDate                    .replace('T00:00:00', '');
      this.dispatch.eTAtoHKG                         = this.dispatch.eTAtoHKG                        .replace('T00:00:00', '');
    }
  }

  save(): void {
    this.saving = true;

    if(this.dispatch.id != undefined){
      this._dispatchService.update(this.dispatch).subscribe(
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
      this._dispatchService.create(this.dispatch).subscribe(
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
