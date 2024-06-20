import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { EmailContentDto } from '../../../shared/service-proxies/emailcontents/model';
import { EmailContentService } from '../../../shared/service-proxies/emailcontents/emailcontent.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-emailcontent',
  templateUrl: './create-update-emailcontent.component.html',
  styleUrls: ['./create-update-emailcontent.component.css']
})
export class CreateUpdateEmailContentComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  emailcontent?: EmailContentDto = {} as EmailContentDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _emailcontentService: EmailContentService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.emailcontent.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.emailcontent.id != undefined){
      this._emailcontentService.update(this.emailcontent).subscribe(
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
      this._emailcontentService.create(this.emailcontent).subscribe(
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
