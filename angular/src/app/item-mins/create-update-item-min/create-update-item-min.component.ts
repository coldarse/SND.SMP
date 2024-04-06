import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { ItemMinDto } from '../../../shared/service-proxies/item-mins/model';
import { ItemMinService } from '../../../shared/service-proxies/item-mins/item-min.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-itemmin',
  templateUrl: './create-update-item-min.component.html',
  styleUrls: ['./create-update-item-min.component.css']
})
export class CreateUpdateItemMinComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  itemmin?: ItemMinDto = {} as ItemMinDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _itemminService: ItemMinService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.itemmin.id != undefined){
      this.isCreate = false;
      this.itemmin.dispatchDate                   = this.itemmin.dispatchDate                  .replace('T00:00:00', '');
    }
  }

  save(): void {
    this.saving = true;

    if(this.itemmin.id != undefined){
      this._itemminService.update(this.itemmin).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
          let cc: BsModalRef;
          cc = this._modalService.show(
            ErrorModalComponent,
            {
              class: 'modal-lg',
              initialState: {
                title: "",
                errorMessage: error.message,
              },
            }
          )
        },
        () => {
          this.saving = false;
        }
      );
    }
    else{
      this._itemminService.create(this.itemmin).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
          let cc: BsModalRef;
          cc = this._modalService.show(
            ErrorModalComponent,
            {
              class: 'modal-lg',
              initialState: {
                title: "",
                errorMessage: error.message,
              },
            }
          )
        },
        () => {
          this.saving = false;
        }
      );
    }

  }

}
