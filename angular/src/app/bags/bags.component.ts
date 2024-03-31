import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { BagDto } from '@shared/service-proxies/bags/model'
import { BagService } from '@shared/service-proxies/bags/bag.service'
import { CreateUpdateBagComponent } from '../bags/create-update-bag/create-update-bag.component'

class PagedBagsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-bags',
  templateUrl: './bags.component.html',
  styleUrls: ['./bags.component.css']
})
export class BagsComponent extends PagedListingComponentBase<BagDto> {

  keyword = '';
  bags: any[] = [];

  constructor(
    injector: Injector,
    private _bagService: BagService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createBag(){
    this.showCreateOrEditBagDialog();
  }

  editBag(entity: BagDto){
    this.showCreateOrEditBagDialog(entity);
  }

  private showCreateOrEditBagDialog(entity?: BagDto){
    let createOrEditBagDialog: BsModalRef;
    if(!entity){
      createOrEditBagDialog = this._modalService.show(
        CreateUpdateBagComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditBagDialog = this._modalService.show(
        CreateUpdateBagComponent,
        {
          class: 'modal-lg',
          initialState: {
            bag: entity
          },
        }
      );
    }

    createOrEditBagDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: BagDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._bagService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Bag.' + action);
  }

  protected list(
    request: PagedBagsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._bagService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.bags = [];
        result.result.items.forEach((element: BagDto) => {

          let tempBag = {
            id: element.id,
            bagNo                         : element.bagNo                         ,
            dispatchId                    : element.dispatchId                    ,
            countryCode                   : element.countryCode                   ,
            weightPre                     : element.weightPre                     ,
            weightPost                    : element.weightPost                    ,
            itemCountPre                  : element.itemCountPre                  ,
            itemCountPost                 : element.itemCountPost                 ,
            weightVariance                : element.weightVariance                ,
            cN35No                        : element.cN35No                        ,
            underAmount                   : element.underAmount                   ,
          }

          this.bags.push(tempBag);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
