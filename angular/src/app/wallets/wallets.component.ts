import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { EWalletDto, WalletDto } from '@shared/service-proxies/wallets/model'
import { WalletService } from '@shared/service-proxies/wallets/wallet.service'
import { CreateUpdateWalletComponent } from '../wallets/create-update-wallet/create-update-wallet.component'
import { EWalletTypeService } from '@shared/service-proxies/ewallettypes/ewallettype.service';
import { CurrencyService } from '@shared/service-proxies/currencies/currency.service';
import { CustomerDto } from '@shared/service-proxies/customers/model';
import { CustomerService } from '@shared/service-proxies/customers/customer.service';

class PagedWalletsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-wallets',
  templateUrl: './wallets.component.html',
  styleUrls: ['./wallets.component.css']
})
export class WalletsComponent extends PagedListingComponentBase<WalletDto> {

  keyword = '';
  wallets: any[] = [];
  isAdmin = true;

  constructor(
    injector: Injector,
    private _walletService: WalletService,
    private _eWalletTypeService: EWalletTypeService,
    private _currencyService: CurrencyService,
    private _customerService: CustomerService,
    private _modalService: BsModalService
  ){
    super(injector);
    this.isAdmin = this.appSession.getShownLoginName().replace('.\\', '').includes('admin') ? true : false;
  }

  createWallet(){
    let entity : WalletDto = {
      customer: '',
      eWalletType: 0,
      currency: 0,
      balance: 0,
      id: 'create'
    }
    this._walletService.getEWalletAsync(entity).subscribe((result: any) => {
      this._customerService.getAllCustomers().subscribe((customers: any) => {
        let custs = customers.result;
        if(!this.isAdmin){
          custs = custs.filter((x: CustomerDto) => x.code == this.appSession.getCompanyCode())
        }
        this.showCreateOrEditWalletDialog(result.result, custs);
      });
    });
  }

  editWallet(entity: WalletDto){
    this._walletService.getEWalletAsync(entity).subscribe((result: any) => {
      this._customerService.getAllCustomers().subscribe((customers: any) => {
        let custs = customers.result;
        if(!this.isAdmin){
          custs = custs.filter((x: CustomerDto) => x.code == this.appSession.getCompanyCode())
        }
        this.showCreateOrEditWalletDialog(result.result, custs);
      });
    });
  }

  private showCreateOrEditWalletDialog(entity: EWalletDto, customerList?: CustomerDto[]){
    let createOrEditWalletDialog: BsModalRef;
    if(entity.id == undefined){
      createOrEditWalletDialog = this._modalService.show(
        CreateUpdateWalletComponent,
        {
          class: 'modal-lg',
          initialState: {
            wallet: entity,
            customerList: customerList
          },
        }
      );
    }
    else{
      createOrEditWalletDialog = this._modalService.show(
        CreateUpdateWalletComponent,
        {
          class: 'modal-lg',
          initialState: {
            wallet: entity,
            customerList: customerList
          },
        }
      );
    }

    createOrEditWalletDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: WalletDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._walletService.deleteEWalletAsync(entity).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Wallet.' + action);
  }

  protected list(
    request: PagedWalletsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {

    if(!this.isAdmin) this.keyword = this.appSession.getCompanyCode();

    request.keyword = this.keyword;
    this._walletService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.wallets = [];
      this._eWalletTypeService.getEWalletTypes().subscribe((ewallettypes: any) => {
        this._currencyService.getCurrencies().subscribe((currencies: any) => {
          result.result.items.forEach((element: WalletDto) => {
            const ewallettype = ewallettypes.result.find(x => x.id === element.eWalletType);
            const currency = currencies.result.find(x => x.id === element.currency);

            let tempWallet = {
              customer: element.customer,
              eWalletType: element.eWalletType,
              eWalletTypeDesc: ewallettype.type,
              currency: element.currency,
              currencyDesc: currency.abbr,
              balance: element.balance,
              id: element.id
            }

            this.wallets.push(tempWallet);
          });
          this.showPaging(result.result, pageNumber);
        });
      });
    });
  }
}
