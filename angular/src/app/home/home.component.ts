import { Component, Injector, OnInit } from "@angular/core";
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
import { DispatchValidationService } from "@shared/service-proxies/dispatchvalidations/dispatchvalidation.service";
import { DispatchValidationDto } from "@shared/service-proxies/dispatchvalidations/model";

class PagedCustomerTransactionsRequestDto extends PagedRequestDto {
  keyword: string;
  isAdmin: boolean;
  customer: string;
}

class PagedDispatchValidationsRequestDto extends PagedRequestDto {
  keyword: string;
  isAdmin: boolean;
  customerCode: string;
}

@Component({
  templateUrl: "./home.component.html",
  animations: [appModuleAnimation()],
  styleUrls: ["./home.component.css"],
})
export class HomeComponent
  extends PagedListingComponentBase<CustomerTransactionDto>
{
  
  customerTransactions: any[] = [];
  dispatchvalidations: any[] = [];
  wallets: DetailedEWallet[] = [];

  isAdmin = true;

  keyword = "";
  showLoginName = "";
  companyCode = "";

  type1 = { eWalletType: 1 };
  type2 = { eWalletType: 2 };

  reloadDispatchValidation: any;

  constructor(
    injector: Injector,
    private _customerTransactionService: CustomerTransactionService,
    private _dispatchvalidationService: DispatchValidationService,
    private _walletService: WalletService
  ) {
    super(injector);
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  startReloadInterval(){
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
    const req = new PagedDispatchValidationsRequestDto();
    req.maxResultCount = this.pageSize;
    req.skipCount = (1 - 1) * this.pageSize;
    req.keyword = this.keyword;
    req.isAdmin = this.isAdmin;
    req.customerCode = this.companyCode;
    this.GetDispatchValidation(req, 1, () => {});
    this.reloadDispatchValidation = setInterval(() => {
      this.GetDispatchValidation(req, 1, () => {});
    }, 30000);
  }

  ngOnDestroy(): void {
    clearInterval(this.reloadDispatchValidation);
  }

  protected list(
    request: PagedCustomerTransactionsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    let admin = this.appSession
      .getShownLoginName()
      .replace(".\\", "")
      .includes("admin");
    this.isAdmin = admin;
    this.companyCode = admin ? "" : this.appSession.getCompanyCode();
    
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
        this.startReloadInterval();
        this._walletService
          .getAllWalletsAsync(this.companyCode)
          .subscribe((result: any) => {
            this.wallets = result.result;
            this.showPaging(result.result, pageNumber);
          });
      });
  }

  private GetDispatchValidation(
    request: PagedDispatchValidationsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._dispatchvalidationService
      .getAll(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.dispatchvalidations = [];
        result.result.items.forEach((element: DispatchValidationDto) => {
          let tempDispatchValidation = {
            id: element.id,
            customerCode: element.customerCode,
            dateStarted: element.dateStarted,
            dateCompleted: element.dateCompleted,
            dispatchNo: element.dispatchNo,
            filePath: element.filePath,
            isFundLack: element.isFundLack,
            isValid: element.isValid,
            postalCode: element.postalCode,
            serviceCode: element.serviceCode,
            productCode: element.productCode,
            status: element.status,
            tookInSec: element.tookInSec,
            validationProgress: element.validationProgress,
          };

          this.dispatchvalidations.push(tempDispatchValidation);
        });
        this.showPaging(result.result, pageNumber);
      });
  }

  protected delete(entity: CustomerTransactionDto): void {
    throw new Error("Method not implemented.");
  }

  filterType(type: number) {
    return this.wallets.filter((w) => w.eWalletType === type);
  }

  entries(event: any) {
    this.pageSize = event.target.value;
    this.getDataPage(1);
  }
}
