import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { ItemDto } from '../../../shared/service-proxies/items/model';
import { ItemService } from '../../../shared/service-proxies/items/item.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

@Component({
  selector: 'app-create-update-item',
  templateUrl: './create-update-item.component.html',
  styleUrls: ['./create-update-item.component.css']
})
export class CreateUpdateItemComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  item?: ItemDto = {} as ItemDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _itemService: ItemService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.item.id != undefined){
      this.isCreate = false;
      this.item.dispatchDate                   = this.item.dispatchDate                  .replace('T00:00:00', '');
    }
  }

  save(): void {
    this.saving = true;

    if(this.item.id != undefined){
      this._itemService.update(this.item).subscribe(
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
      this._itemService.create(this.item).subscribe(
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
