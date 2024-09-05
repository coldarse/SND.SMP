import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { RateZoneDto } from '../../../shared/service-proxies/ratezones/model';
import { RateZoneService } from '../../../shared/service-proxies/ratezones/ratezone.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-ratezone',
  templateUrl: './create-update-ratezone.component.html',
  styleUrls: ['./create-update-ratezone.component.css']
})
export class CreateUpdateRateZoneComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  ratezone?: RateZoneDto = {} as RateZoneDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _ratezoneService: RateZoneService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.ratezone.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.ratezone.id != undefined){
      this._ratezoneService.update(this.ratezone).subscribe(
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
      this._ratezoneService.create(this.ratezone).subscribe(
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
