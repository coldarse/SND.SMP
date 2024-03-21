import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { RateWeightBreakDto } from '../../../shared/service-proxies/rateweightbreaks/model';
import { RateWeightBreakService } from '../../../shared/service-proxies/rateweightbreaks/rateweightbreak.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-rateweightbreak',
  templateUrl: './create-update-rateweightbreak.component.html',
  styleUrls: ['./create-update-rateweightbreak.component.css']
})
export class CreateUpdateRateWeightBreakComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  rateweightbreak?: RateWeightBreakDto = {} as RateWeightBreakDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _rateweightbreakService: RateWeightBreakService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.rateweightbreak.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.rateweightbreak.id != undefined){
      this._rateweightbreakService.update(this.rateweightbreak).subscribe(
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
      this._rateweightbreakService.create(this.rateweightbreak).subscribe(
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
