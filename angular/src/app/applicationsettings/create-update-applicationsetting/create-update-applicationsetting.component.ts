import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { ApplicationSettingDto } from '../../../shared/service-proxies/applicationsettings/model';
import { ApplicationSettingService } from '../../../shared/service-proxies/applicationsettings/applicationsetting.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-applicationsetting',
  templateUrl: './create-update-applicationsetting.component.html',
  styleUrls: ['./create-update-applicationsetting.component.css']
})
export class CreateUpdateApplicationSettingComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  applicationsetting?: ApplicationSettingDto = {} as ApplicationSettingDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _applicationsettingService: ApplicationSettingService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.applicationsetting.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.applicationsetting.id != undefined){
      this._applicationsettingService.update(this.applicationsetting).subscribe(
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
      this._applicationsettingService.create(this.applicationsetting).subscribe(
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
