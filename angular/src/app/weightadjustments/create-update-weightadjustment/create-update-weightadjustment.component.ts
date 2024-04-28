import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { WeightAdjustmentDto } from '../../../shared/service-proxies/weightadjustments/model';
import { WeightAdjustmentService } from '../../../shared/service-proxies/weightadjustments/weightadjustment.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-weightadjustment',
  templateUrl: './create-update-weightadjustment.component.html',
  styleUrls: ['./create-update-weightadjustment.component.css']
})
export class CreateUpdateWeightAdjustmentComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  weightadjustment?: WeightAdjustmentDto = {} as WeightAdjustmentDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _weightadjustmentService: WeightAdjustmentService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.weightadjustment.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.weightadjustment.id != undefined){
      this._weightadjustmentService.update(this.weightadjustment).subscribe(
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
      this._weightadjustmentService.create(this.weightadjustment).subscribe(
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
