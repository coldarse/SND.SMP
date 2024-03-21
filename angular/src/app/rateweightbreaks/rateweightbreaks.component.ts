import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { RateWeightBreakDto } from '@shared/service-proxies/rateweightbreaks/model'
import { RateWeightBreakService } from '@shared/service-proxies/rateweightbreaks/rateweightbreak.service'
import { CreateUpdateRateWeightBreakComponent } from '../rateweightbreaks/create-update-rateweightbreak/create-update-rateweightbreak.component'

class PagedRateWeightBreaksRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-rateweightbreaks',
  templateUrl: './rateweightbreaks.component.html',
  styleUrls: ['./rateweightbreaks.component.css']
})
export class RateWeightBreaksComponent extends PagedListingComponentBase<RateWeightBreakDto> {

  keyword = '';
  rateweightbreaks: any[] = [];

  constructor(
    injector: Injector,
    private _rateweightbreakService: RateWeightBreakService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createRateWeightBreak(){
    this.showCreateOrEditRateWeightBreakDialog();
  }

  editRateWeightBreak(entity: RateWeightBreakDto){
    this.showCreateOrEditRateWeightBreakDialog(entity);
  }

  private showCreateOrEditRateWeightBreakDialog(entity?: RateWeightBreakDto){
    let createOrEditRateWeightBreakDialog: BsModalRef;
    if(!entity){
      createOrEditRateWeightBreakDialog = this._modalService.show(
        CreateUpdateRateWeightBreakComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditRateWeightBreakDialog = this._modalService.show(
        CreateUpdateRateWeightBreakComponent,
        {
          class: 'modal-lg',
          initialState: {
            rateweightbreak: entity
          },
        }
      );
    }

    createOrEditRateWeightBreakDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: RateWeightBreakDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._rateweightbreakService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.RateWeightBreak.' + action);
  }

  protected list(
    request: PagedRateWeightBreaksRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._rateweightbreakService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.rateweightbreaks = [];
        result.result.items.forEach((element: RateWeightBreakDto) => {

          let tempRateWeightBreak = {
            id: element.id,
            rateId: element.rateId,
            postalOrgId: element.postalOrgId,
            weightMin: element.weightMin,
            weightMax: element.weightMax,
            productCode: element.productCode,
            currencyId: element.currencyId,
            itemRate: element.itemRate,
            weightRate: element.weightRate,
            isExceedRule: element.isExceedRule,
            paymentMode: element.paymentMode,
          }

          this.rateweightbreaks.push(tempRateWeightBreak);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
