import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { WeightAdjustmentDto } from '@shared/service-proxies/weightadjustments/model'
import { WeightAdjustmentService } from '@shared/service-proxies/weightadjustments/weightadjustment.service'
import { CreateUpdateWeightAdjustmentComponent } from '../weightadjustments/create-update-weightadjustment/create-update-weightadjustment.component'

class PagedWeightAdjustmentsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-weightadjustments',
  templateUrl: './weightadjustments.component.html',
  styleUrls: ['./weightadjustments.component.css']
})
export class WeightAdjustmentsComponent extends PagedListingComponentBase<WeightAdjustmentDto> {

  keyword = '';
  weightadjustments: any[] = [];

  constructor(
    injector: Injector,
    private _weightadjustmentService: WeightAdjustmentService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createWeightAdjustment(){
    this.showCreateOrEditWeightAdjustmentDialog();
  }

  editWeightAdjustment(entity: WeightAdjustmentDto){
    this.showCreateOrEditWeightAdjustmentDialog(entity);
  }

  private showCreateOrEditWeightAdjustmentDialog(entity?: WeightAdjustmentDto){
    let createOrEditWeightAdjustmentDialog: BsModalRef;
    if(!entity){
      createOrEditWeightAdjustmentDialog = this._modalService.show(
        CreateUpdateWeightAdjustmentComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditWeightAdjustmentDialog = this._modalService.show(
        CreateUpdateWeightAdjustmentComponent,
        {
          class: 'modal-lg',
          initialState: {
            weightadjustment: entity
          },
        }
      );
    }

    createOrEditWeightAdjustmentDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: WeightAdjustmentDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._weightadjustmentService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.WeightAdjustment.' + action);
  }

  protected list(
    request: PagedWeightAdjustmentsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._weightadjustmentService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.weightadjustments = [];
        result.result.items.forEach((element: WeightAdjustmentDto) => {

          let tempWeightAdjustment = {
            id: element.id,
            userId: element.userId,
            referenceNo: element.referenceNo,
            amount: element.amount,
            description: element.description,
            dateTime: element.dateTime,
            weight: element.weight,
            invoiceId: element.invoiceId,
          }

          this.weightadjustments.push(tempWeightAdjustment);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
