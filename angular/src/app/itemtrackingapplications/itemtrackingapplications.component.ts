import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { ItemTrackingApplicationDto } from '@shared/service-proxies/itemtrackingapplications/model'
import { ItemTrackingApplicationService } from '@shared/service-proxies/itemtrackingapplications/itemtrackingapplication.service'
import { CreateUpdateItemTrackingApplicationComponent } from '../itemtrackingapplications/create-update-itemtrackingapplication/create-update-itemtrackingapplication.component'

class PagedItemTrackingApplicationsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-itemtrackingapplications',
  templateUrl: './itemtrackingapplications.component.html',
  styleUrls: ['./itemtrackingapplications.component.css']
})
export class ItemTrackingApplicationsComponent extends PagedListingComponentBase<ItemTrackingApplicationDto> {

  keyword = '';
  itemtrackingapplications: any[] = [];

  constructor(
    injector: Injector,
    private _itemtrackingapplicationService: ItemTrackingApplicationService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createItemTrackingApplication(){
    this.showCreateOrEditItemTrackingApplicationDialog();
  }

  editItemTrackingApplication(entity: ItemTrackingApplicationDto){
    this.showCreateOrEditItemTrackingApplicationDialog(entity);
  }

  private showCreateOrEditItemTrackingApplicationDialog(entity?: ItemTrackingApplicationDto){
    let createOrEditItemTrackingApplicationDialog: BsModalRef;
    if(!entity){
      createOrEditItemTrackingApplicationDialog = this._modalService.show(
        CreateUpdateItemTrackingApplicationComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditItemTrackingApplicationDialog = this._modalService.show(
        CreateUpdateItemTrackingApplicationComponent,
        {
          class: 'modal-lg',
          initialState: {
            itemtrackingapplication: entity
          },
        }
      );
    }

    createOrEditItemTrackingApplicationDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: ItemTrackingApplicationDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._itemtrackingapplicationService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.ItemTrackingApplication.' + action);
  }

  protected list(
    request: PagedItemTrackingApplicationsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._itemtrackingapplicationService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.itemtrackingapplications = [];
        result.result.items.forEach((element: ItemTrackingApplicationDto) => {

          let tempItemTrackingApplication = {
            id: element.id,
            customerId: element.customerId,
            customerCode: element.customerCode,
            postalCode: element.postalCode,
            postalDesc: element.postalDesc,
            total: element.total,
            productCode: element.productCode,
            productDesc: element.productDesc,
            status: element.status,
            dateCreated: element.dateCreated,
          }

          this.itemtrackingapplications.push(tempItemTrackingApplication);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
