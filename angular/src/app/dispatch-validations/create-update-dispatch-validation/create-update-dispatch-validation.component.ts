import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { DispatchValidationDto } from '../../../shared/service-proxies/dispatch-validations/model';
import { DispatchValidationService } from '../../../shared/service-proxies/dispatch-validations/dispatch-validation.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-dispatch-validation',
  templateUrl: './create-update-dispatch-validation.component.html',
  styleUrls: ['./create-update-dispatch-validation.component.css']
})
export class CreateUpdateDispatchValidationComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  dispatchvalidation?: DispatchValidationDto = {} as DispatchValidationDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _dispatchvalidationService: DispatchValidationService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.dispatchvalidation.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.dispatchvalidation.id != undefined){
      this._dispatchvalidationService.update(this.dispatchvalidation).subscribe(
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
      this._dispatchvalidationService.create(this.dispatchvalidation).subscribe(
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
