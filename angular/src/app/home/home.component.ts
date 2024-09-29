import { Component, EventEmitter, Injector, Input, OnChanges, OnInit, Output } from "@angular/core";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { PagedListingComponentBase } from "@shared/paged-listing-component-base";

import { WalletService } from "@shared/service-proxies/wallets/wallet.service";
import { DetailedEWallet } from "@shared/service-proxies/wallets/model";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  animations: [appModuleAnimation()],
  styleUrls: ["./home.component.css"],
})
export class HomeComponent
  extends PagedListingComponentBase<any>
  implements OnInit
{
  wallets: DetailedEWallet[] = [];

  isAdmin = true;

  @Input() fromCustomerInfo = false;

  keyword = "";
  showLoginName = "";
  companyCode = "";

  @Input() specific_showLoginName : string;
  @Input() specific_companyCode : string;

  @Output() closeEmit = new EventEmitter<any>();

  type1 = { eWalletType: 1 };
  type2 = { eWalletType: 2 };
  type3 = { eWalletType: 3 };

  showTable = "1";

  constructor(injector: Injector, private _walletService: WalletService) {
    super(injector);
  }

  ngOnInit(): void {
    if (this.fromCustomerInfo) {
      this.isAdmin = false;
      this.showLoginName = this.specific_showLoginName;
      this.companyCode = this.specific_companyCode;
      this._walletService
        .getAllWalletsAsync(this.companyCode)
        .subscribe((result: any) => {
          this.wallets = result.result;
        });
    } else {
      this.isAdmin = this.appSession
        .getShownLoginName()
        .replace(".\\", "")
        .includes("admin");
      this.showLoginName = this.appSession
        .getShownLoginName()
        .replace(".\\", "");
      this.companyCode = this.appSession.getCompanyCode();

      this._walletService
        .getAllWalletsAsync(this.companyCode)
        .subscribe((result: any) => {
          this.wallets = result.result;
        });
    }
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  closeDashboard(): void {
    this.closeEmit.emit();
  }

  protected list(
    request: any,
    pageNumber: number,
    finishedCallback: Function
  ): void {}

  protected delete(entity: any): void {
    throw new Error("Method not implemented.");
  }

  filterType(type: number) {
    return this.wallets.filter((w) => w.eWalletType === type);
  }

  showSelectedTable(value: string) {
    this.showTable = value;
  }
}
