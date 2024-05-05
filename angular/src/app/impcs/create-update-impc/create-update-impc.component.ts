import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { IMPCDto } from '../../../shared/service-proxies/impcs/model';
import { IMPCService } from '../../../shared/service-proxies/impcs/impc.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-impc',
  templateUrl: './create-update-impc.component.html',
  styleUrls: ['./create-update-impc.component.css']
})
export class CreateUpdateIMPCComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  impc?: IMPCDto = {} as IMPCDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _impcService: IMPCService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.impc.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.impc.id != undefined){
      this._impcService.update(this.impc).subscribe(
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
      this._impcService.create(this.impc).subscribe(
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
