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
  TopUpEWalletDto,
  WalletDto,
} from "../../../shared/service-proxies/wallets/model";
import { WalletService } from "../../../shared/service-proxies/wallets/wallet.service";
import { BsModalRef } from "ngx-bootstrap/modal";

@Component({
  selector: "app-topup-wallet",
  templateUrl: "./manage-credit.component.html",
  styleUrls: ["./manage-credit.component.css"],
})
export class ManageCreditComponent extends AppComponentBase implements OnInit {
  saving = false;
  isCreate = true;
  wallet?: EWalletDto = {} as EWalletDto;

  referenceNo: string = '';
  description: string = '';
  amount: number = 0;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _walletService: WalletService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
  }

  save(): void {
    this.saving = true;

    const topUpBody : TopUpEWalletDto = {
      eWalletType: this.wallet.eWalletTypeDesc,
      currency: this.wallet.currencyDesc,
      referenceNo: this.referenceNo,
      description: this.description,
      amount: this.amount,
      id: this.wallet.id
    }

    this._walletService.manageCredit(topUpBody).subscribe(
      () => {
        this.notify.info(this.l("SavedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      },
      () => {
        this.saving = false;
      }
    );
  }
}
