import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { RefundDto } from '../../../shared/service-proxies/refunds/model';
import { RefundService } from '../../../shared/service-proxies/refunds/refund.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-refund',
  templateUrl: './create-update-refund.component.html',
  styleUrls: ['./create-update-refund.component.css']
})
export class CreateUpdateRefundComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  refund?: RefundDto = {} as RefundDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _refundService: RefundService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.refund.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.refund.id != undefined){
      this._refundService.update(this.refund).subscribe(
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
      this._refundService.create(this.refund).subscribe(
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
