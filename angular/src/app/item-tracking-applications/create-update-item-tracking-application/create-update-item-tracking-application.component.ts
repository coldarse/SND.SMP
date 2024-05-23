import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { ItemTrackingApplicationDto } from '../../../shared/service-proxies/item-tracking-applications/model';
import { ItemTrackingApplicationService } from '../../../shared/service-proxies/item-tracking-applications/item-tracking-application.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-item-tracking-application',
  templateUrl: './create-update-item-tracking-application.component.html',
  styleUrls: ['./create-update-item-tracking-application.component.css']
})
export class CreateUpdateItemTrackingApplicationComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  itemtrackingapplication?: ItemTrackingApplicationDto = {} as ItemTrackingApplicationDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _itemtrackingapplicationService: ItemTrackingApplicationService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.itemtrackingapplication.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.itemtrackingapplication.id != undefined){
      this._itemtrackingapplicationService.update(this.itemtrackingapplication).subscribe(
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
      this._itemtrackingapplicationService.create(this.itemtrackingapplication).subscribe(
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
