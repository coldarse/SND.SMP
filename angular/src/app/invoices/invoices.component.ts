import { Component, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { InvoiceDto } from "@shared/service-proxies/invoices/model";
import { InvoiceService } from "@shared/service-proxies/invoices/invoice.service";
import { CreateUpdateInvoiceComponent } from "../invoices/create-update-invoice/create-update-invoice.component";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";
import { CustomerCurrency, CustomerDto } from "@shared/service-proxies/customers/model";
import { CurrencyDto } from "@shared/service-proxies/currencies/model";
import { CurrencyService } from "@shared/service-proxies/currencies/currency.service";
import { ApplicationSettingService } from "@shared/service-proxies/applicationsettings/applicationsetting.service";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";

class PagedInvoicesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  selector: "app-invoices",
  templateUrl: "./invoices.component.html",
  styleUrls: ["./invoices.component.css"],
})
export class InvoicesComponent extends PagedListingComponentBase<InvoiceDto> {
  keyword = "";
  invoices: any[] = [];

  customers: CustomerCurrency[] = [];
  currencies: CurrencyDto[] = [];

  constructor(
    injector: Injector,
    private _invoiceService: InvoiceService,
    private _modalService: BsModalService,
    private _customerService: CustomerService,
    private _currencyService: CurrencyService,
    private _chibiService: ChibiService,
    private _applicationSettingService: ApplicationSettingService
  ) {
    super(injector);
  }

  createInvoice() {
    this.showCreateOrEditInvoiceDialog();
  }

  private showCreateOrEditInvoiceDialog() {
    this._applicationSettingService
      .getValueByName("InvoiceNo")
      .subscribe((data: any) => {
        let invoice_no = (data.result == "" ? 0 : +data.result);

        let createOrEditInvoiceDialog: BsModalRef;
        createOrEditInvoiceDialog = this._modalService.show(
          CreateUpdateInvoiceComponent,
          {
            class: "modal-xl",
            initialState: {
              customers: this.customers,
              currencies: this.currencies,
              runningNo: invoice_no.toString(),
            },
          }
        );

        createOrEditInvoiceDialog.content.onSave.subscribe(() => {
          this.refresh();
        });
      });
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  protected delete(entity: InvoiceDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._chibiService.deleteInvoice(entity.invoiceUrl).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.Invoice." + action);
  }

  protected list(
    request: PagedInvoicesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._invoiceService
      .getAll(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.invoices = [];
        result.result.items.forEach((element: InvoiceDto) => {
          let invoice_split = element.invoiceNo.split("|");
          let invoice_no = invoice_split[0];
          let invoice_url = invoice_split[1];

          let tempInvoice = {
            id: element.id,
            dateTime: element.dateTime,
            invoiceNo: invoice_no,
            customer: element.customer,
            invoiceUrl: invoice_url,
          };

          this.invoices.push(tempInvoice);
        });
        this.showPaging(result.result, pageNumber);
        this._customerService.getCustomers().subscribe((cust: any) => {
          this.customers = cust.result;
          this._currencyService.getCurrencies().subscribe((curr: any) => {
            this.currencies = curr.result;
          });
        });
      });
  }
}
