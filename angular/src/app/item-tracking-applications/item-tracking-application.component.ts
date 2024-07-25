import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { ItemTrackingApplicationDto } from '@shared/service-proxies/item-tracking-applications/model'
import { ItemTrackingApplicationService } from '@shared/service-proxies/item-tracking-applications/item-tracking-application.service'
import { CreateUpdateItemTrackingApplicationComponent } from '../item-tracking-applications/create-update-item-tracking-application/create-update-item-tracking-application.component';
import { CreateItemTrackingApplicationComponent } from './create-item-tracking-application/create-item-tracking-application.component';
import { ReviewItemTrackingApplicationComponent } from './review-item-tracking-application/review-item-tracking-application.component';
import { ChibiService } from '@shared/service-proxies/chibis/chibis.service';
import { HttpResponse } from '@angular/common/http';

class PagedItemTrackingApplicationsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-item-tracking-application',
  templateUrl: './item-tracking-application.component.html',
  styleUrls: ['./item-tracking-application.component.css']
})
export class ItemTrackingApplicationsComponent extends PagedListingComponentBase<ItemTrackingApplicationDto> {

  keyword = '';
  itemtrackingapplications: any[] = [];

  constructor(
    injector: Injector,
    private _itemtrackingapplicationService: ItemTrackingApplicationService,
    private _chibiService: ChibiService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createItemTrackingApplication(){
    this.showCreateItemTrackingApplicationDialog();
  }

  editItemTrackingApplication(entity: ItemTrackingApplicationDto){
    this.showReviewItemTrackingApplicationDialog(entity);
  }

  exportItemTrackingIds(entity: ItemTrackingApplicationDto){
    this._chibiService
    .downloadItemTrackingIds(entity.id)
    .pipe(
      finalize(() => {
      })
    )
    .subscribe((res: HttpResponse<Blob>) => {
      var contentDisposition = res.headers.get("content-disposition");
      var filename = contentDisposition
        .split(";")[1]
        .split("filename")[1]
        .split("=")[1]
        .trim();
      console.log(filename);
      const blob = new Blob([res.body], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = filename;
      a.click();

      // Clean up
      window.URL.revokeObjectURL(url);
      abp.notify.success(this.l("Successfully Downloaded"));
    });
  }

  private showCreateItemTrackingApplicationDialog(){
    let createItemTrackingApplicationDialog: BsModalRef;
    createItemTrackingApplicationDialog = this._modalService.show(
      CreateItemTrackingApplicationComponent,
      {
        class: 'modal-lg',
      }
    );

    createItemTrackingApplicationDialog.content.onSave.subscribe(() => {
      this.getDataPage(this.pageNumber);
    });
  }

  private showReviewItemTrackingApplicationDialog(entity: ItemTrackingApplicationDto){
    let reviewItemTrackingApplicationDialog: BsModalRef;
    reviewItemTrackingApplicationDialog = this._modalService.show(
      ReviewItemTrackingApplicationComponent,
      {
        class: 'modal-lg',
        initialState: {
          application: entity
        },
      }
    );

    reviewItemTrackingApplicationDialog.content.onSave.subscribe(() => {
      this.getDataPage(this.pageNumber);
    });
    
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
      this.getDataPage(this.pageNumber);
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
            range: element.range,
          }

          this.itemtrackingapplications.push(tempItemTrackingApplication);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
