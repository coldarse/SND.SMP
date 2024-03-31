import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { DispatchValidationDto } from '../../../shared/service-proxies/dispatchvalidations/model';
import { DispatchValidationService } from '../../../shared/service-proxies/dispatchvalidations/dispatchvalidation.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-dispatchvalidation',
  templateUrl: './create-update-dispatchvalidation.component.html',
  styleUrls: ['./create-update-dispatchvalidation.component.css']
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
    public bsModalRef: BsModalRef
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
        () => {
          this.saving = false;
        }
      );
    }

  }

}
