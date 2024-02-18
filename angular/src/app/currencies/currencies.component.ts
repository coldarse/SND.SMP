import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { CurrencyDto } from '@shared/service-proxies/currencies/model'
import { CurrencyService } from '@shared/service-proxies/currencies/currency.service'
import { CreateUpdateCurrencyComponent } from '../currencies/create-update-currency/create-update-currency.component'

class PagedCurrenciesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-currencies',
  templateUrl: './currencies.component.html',
  styleUrls: ['./currencies.component.css']
})
export class CurrenciesComponent extends PagedListingComponentBase<CurrencyDto> {

  keyword = '';
  currencies: any[] = [];

  constructor(
    injector: Injector,
    private _currencyService: CurrencyService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createCurrency(){
    this.showCreateOrEditCurrencyDialog();
  }

  editCurrency(entity: CurrencyDto){
    this.showCreateOrEditCurrencyDialog(entity);
  }

  private showCreateOrEditCurrencyDialog(entity?: CurrencyDto){
    let createOrEditCurrencyDialog: BsModalRef;
    if(!entity){
      createOrEditCurrencyDialog = this._modalService.show(
        CreateUpdateCurrencyComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditCurrencyDialog = this._modalService.show(
        CreateUpdateCurrencyComponent,
        {
          class: 'modal-lg',
          initialState: {
            currency: entity
          },
        }
      );
    }

    createOrEditCurrencyDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: CurrencyDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._currencyService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Currency.' + action);
  }

  protected list(
    request: PagedCurrenciesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._currencyService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.currencies = [];
        result.result.items.forEach((element: CurrencyDto) => {

          let tempCurrency = {
            id: element.id,
            abbr: element.abbr,
            description: element.description,
          }

          this.currencies.push(tempCurrency);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
