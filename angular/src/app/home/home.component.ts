import { Component, Injector } from "@angular/core";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { CustomerTransactionDto } from "@shared/service-proxies/customer-transactions/model";
import { CustomerTransactionService } from "@shared/service-proxies/customer-transactions/customer-transaction.service";
import { finalize } from "rxjs";
import { WalletService } from "@shared/service-proxies/wallets/wallet.service";
import {
  DetailedEWallet,
  WalletDto,
} from "@shared/service-proxies/wallets/model";

class PagedCustomerTransactionsRequestDto extends PagedRequestDto {
  keyword: string;
  isAdmin: boolean;
  customer: string;
}


@Component({
  templateUrl: "./home.component.html",
  animations: [appModuleAnimation()],
  styleUrls: ["./home.component.css"],
})
export class HomeComponent extends PagedListingComponentBase<CustomerTransactionDto> {
  keyword = "";
  customerTransactions: any[] = [];
  isAdmin = true;
  showLoginName = "";
  companyCode = "";
  wallets: DetailedEWallet[] = [];

  type1 = { eWalletType: 1 };
  type2 = { eWalletType: 2 };

  protected list(
    request: PagedCustomerTransactionsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (
      this.appSession.getShownLoginName().replace(".\\", "").includes("admin")
    ) {
      this.isAdmin = true;
      this.showLoginName = this.appSession
        .getShownLoginName()
        .replace(".\\", "");
      this.companyCode = "";
    } else {
      this.isAdmin = false;
      this.showLoginName = this.appSession.getShownCustomerCompanyName();
      this.companyCode = this.appSession.getCompanyCode();
    }
    request.keyword = this.keyword;
    request.isAdmin = this.isAdmin;
    request.customer = this.companyCode;
    this._customerTransactionService
      .getAll(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.customerTransactions = [];
        result.result.items.forEach((element: CustomerTransactionDto) => {
          let tempCustomerTransaction = {
            wallet: element.wallet,
            customer: element.customer,
            paymentMode: element.paymentMode,
            currency: element.currency,
            transactionType: element.transactionType,
            amount: element.amount,
            referenceNo: element.referenceNo,
            description: element.description,
            transactionDate: element.transactionDate,
          };

          this.customerTransactions.push(tempCustomerTransaction);
        });
        this._walletService
          .getAllWalletsAsync(this.companyCode)
          .subscribe((result: any) => {
            this.wallets = result.result;
            this.showPaging(result.result, pageNumber);
          });
      });
  }
  protected delete(entity: CustomerTransactionDto): void {
    throw new Error("Method not implemented.");
  } 

  filterType(type: number){
    return this.wallets.filter(w => w.eWalletType === type);
  }

  entries(event: any){
    this.pageSize = event.target.value;
    this.getDataPage(1);
  }

  constructor(
    injector: Injector,
    private _customerTransactionService: CustomerTransactionService,
    private _walletService: WalletService
  ) {
    super(injector);
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }
}
