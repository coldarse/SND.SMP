import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { BagDto } from '../../../shared/service-proxies/bags/model';
import { BagService } from '../../../shared/service-proxies/bags/bag.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

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
    public bsModalRef: BsModalRef
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
        () => {
          this.saving = false;
        }
      );
    }

  }

}
