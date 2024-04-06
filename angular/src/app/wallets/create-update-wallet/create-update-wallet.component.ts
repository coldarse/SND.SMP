import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import {
  EWalletDto,
  UpdateWalletDto,
  WalletDto,
} from "../../../shared/service-proxies/wallets/model";
import { WalletService } from "../../../shared/service-proxies/wallets/wallet.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { CustomerDto } from "@shared/service-proxies/customers/model";
import { HttpErrorResponse } from "@angular/common/http";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";

@Component({
  selector: "app-create-update-wallet",
  templateUrl: "./create-update-wallet.component.html",
  styleUrls: ["./create-update-wallet.component.css"],
})
export class CreateUpdateWalletComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  isCreate = true;
  wallet?: EWalletDto = {} as EWalletDto;
  customerList?: CustomerDto[] = [];

  ogCustomer = "";
  ogEWalletType = 0;
  ogCurrency = 0;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _walletService: WalletService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (this.wallet.id != undefined) {
      this.ogCustomer = this.wallet.customer;
      this.ogEWalletType = this.wallet.eWalletType;
      this.ogCurrency = this.wallet.currency;

      this.isCreate = false;
    }
  }

  selectedEWalletType(event: any) {
    this.wallet.eWalletType = event.target.value;
  }

  selectedCurrency(event: any) {
    this.wallet.currency = event.target.value;
  }

  getCurrencyAbbr(id: number){
    return this.wallet.currencyList.find(x => x.id == id).abbr;
  }

  getEWalletTypeAbbr(id: number){
    const ewallettype = this.wallet.eWalletTypeList.find(x => x.id == id).type;

    switch(ewallettype){
      case 'Prepaid':
        return 'PP';
      case 'Credit Term':
        return 'CT';
    }
  }

  save(): void {
    this.saving = true;

    const walletName = this.wallet.customer  + this.getEWalletTypeAbbr(this.wallet.eWalletType) + this.getCurrencyAbbr(this.wallet.currency)

    if (this.wallet.id != null) {
      let update = true;
      if (this.ogCustomer === this.wallet.customer)
        if (this.ogEWalletType === this.wallet.eWalletType)
          if (this.ogCurrency === this.wallet.currency) update = false;

      if (update) {

        let updateValue : UpdateWalletDto = {
          ogCurrency: this.ogCurrency,
          ogeWalletType : this.ogEWalletType,
          ogCustomer: this.ogCustomer,
          currency: this.wallet.currency,
          eWalletType: this.wallet.eWalletType,
          customer: this.wallet.customer,
          id: walletName
        }

        this._walletService.updateEWalletAsync(updateValue).subscribe(
          () => {
            this.notify.info(this.l("SavedSuccessfully"));
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

      } else {
        this.notify.info(this.l("SavedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
        this.saving = false;
      }

    } else {

      let createValue : WalletDto = {
        customer: this.wallet.customer,
        eWalletType: this.wallet.eWalletType,
        currency: this.wallet.currency,
        balance: this.wallet.balance,
        id: walletName
      }

      this._walletService.create(createValue).subscribe(
        () => {
          this.notify.info(this.l("SavedSuccessfully"));
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
