import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Injector, Input, Output } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AbpModalHeaderComponent } from '../modal/abp-modal-header.component';
import { AbpModalFooterComponent } from '../modal/abp-modal-footer.component';
import { SharedModule } from '@shared/shared.module';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-error-modal',
  standalone: true,
  templateUrl: './error-modal.component.html',
  imports: [
    SharedModule
  ]
})
export class ErrorModalComponent extends AppComponentBase {
  
  title: string = "";
  errorMessage: string = "";
  object: any = {};

  yesno: boolean = false;

  @Output() yesClick = new EventEmitter<any>;

  constructor(injector: Injector, public bsModalRef: BsModalRef,) {
    super(injector);
  }

  clickYes(){
    this.yesClick.emit();
    this.bsModalRef.hide();
  }
}