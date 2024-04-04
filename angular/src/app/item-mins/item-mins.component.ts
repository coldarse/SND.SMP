import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { ItemMinDto } from '@shared/service-proxies/item-mins/model'
import { ItemMinService } from '@shared/service-proxies/item-mins/item-min.service'
import { CreateUpdateItemMinComponent } from './create-update-item-min/create-update-item-min.component'

class PagedItemMinsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-item-mins',
  templateUrl: './item-mins.component.html',
  styleUrls: ['./item-mins.component.css']
})
export class ItemMinsComponent extends PagedListingComponentBase<ItemMinDto> {

  keyword = '';
  itemmins: any[] = [];

  constructor(
    injector: Injector,
    private _itemminService: ItemMinService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createItemMin(){
    this.showCreateOrEditItemMinDialog();
  }

  editItemMin(entity: ItemMinDto){
    this.showCreateOrEditItemMinDialog(entity);
  }

  private showCreateOrEditItemMinDialog(entity?: ItemMinDto){
    let createOrEditItemMinDialog: BsModalRef;
    if(!entity){
      createOrEditItemMinDialog = this._modalService.show(
        CreateUpdateItemMinComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditItemMinDialog = this._modalService.show(
        CreateUpdateItemMinComponent,
        {
          class: 'modal-lg',
          initialState: {
            itemmin: entity
          },
        }
      );
    }

    createOrEditItemMinDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: ItemMinDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._itemminService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.ItemMin.' + action);
  }

  protected list(
    request: PagedItemMinsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._itemminService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.itemmins = [];
        result.result.items.forEach((element: ItemMinDto) => {

          let tempItemMin = {
            id: element.id,
            extID                         : element.extID                         ,
            dispatchID                    : element.dispatchID                    ,
            bagID                         : element.bagID                         ,
            dispatchDate                  : element.dispatchDate                  ,
            month                         : element.month                         ,
            countryCode                   : element.countryCode                   ,
            weight                        : element.weight                        ,
            itemValue                     : element.itemValue                     ,
            recpName                      : element.recpName                      ,
            itemDesc                      : element.itemDesc                      ,
            address                       : element.address                       ,
            city                          : element.city                          ,
            telNo                         : element.telNo                         ,
            deliveredInDays               : element.deliveredInDays               ,
            isDelivered                   : element.isDelivered                   ,
            status                        : element.status                        ,
          }

          this.itemmins.push(tempItemMin);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
