import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { RefundDto } from '@shared/service-proxies/refunds/model'
import { RefundService } from '@shared/service-proxies/refunds/refund.service'
import { CreateUpdateRefundComponent } from '../refunds/create-update-refund/create-update-refund.component'

class PagedRefundsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-refunds',
  templateUrl: './refunds.component.html',
  styleUrls: ['./refunds.component.css']
})
export class RefundsComponent extends PagedListingComponentBase<RefundDto> {

  keyword = '';
  refunds: any[] = [];

  constructor(
    injector: Injector,
    private _refundService: RefundService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createRefund(){
    this.showCreateOrEditRefundDialog();
  }

  editRefund(entity: RefundDto){
    this.showCreateOrEditRefundDialog(entity);
  }

  private showCreateOrEditRefundDialog(entity?: RefundDto){
    let createOrEditRefundDialog: BsModalRef;
    if(!entity){
      createOrEditRefundDialog = this._modalService.show(
        CreateUpdateRefundComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditRefundDialog = this._modalService.show(
        CreateUpdateRefundComponent,
        {
          class: 'modal-lg',
          initialState: {
            refund: entity
          },
        }
      );
    }

    createOrEditRefundDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: RefundDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._refundService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Refund.' + action);
  }

  protected list(
    request: PagedRefundsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._refundService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.refunds = [];
        result.result.items.forEach((element: RefundDto) => {

          let tempRefund = {
            id: element.id,
            userId: element.userId,
            referenceNo: element.referenceNo,
            amount: element.amount,
            description: element.description,
            dateTime: element.dateTime,
            weight: element.weight,
          }

          this.refunds.push(tempRefund);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
