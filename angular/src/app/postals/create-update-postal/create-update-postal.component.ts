import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { PostalDto } from '../../../shared/service-proxies/postals/model';
import { PostalService } from '../../../shared/service-proxies/postals/postal.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-postal',
  templateUrl: './create-update-postal.component.html',
  styleUrls: ['./create-update-postal.component.css']
})
export class CreateUpdatePostalComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  postal: PostalDto = {} as PostalDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _postalService: PostalService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.postal.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.postal.id != undefined){
      this._postalService.update(this.postal).subscribe(
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
      this._postalService.create(this.postal).subscribe(
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