import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import {
  ExtraCharge,
  GenerateInvoice,
  InvoiceDto,
} from "../../../shared/service-proxies/invoices/model";
import { InvoiceService } from "../../../shared/service-proxies/invoices/invoice.service";
import { BsModalRef } from "ngx-bootstrap/modal";
import { DatePipe } from "@angular/common";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { CustomerCurrency, CustomerDto } from "@shared/service-proxies/customers/model";
import { CurrencyDto } from "@shared/service-proxies/currencies/model";
import { ApplicationSettingService } from "@shared/service-proxies/applicationsettings/applicationsetting.service";
import {
  CustomerDispatchDetails,
  DispatchDetails,
  ItemWrapper,
  SimplifiedItem,
} from "@shared/service-proxies/dispatches/model";

@Component({
  selector: "app-create-update-invoice",
  templateUrl: "./create-update-invoice.component.html",
  styleUrls: ["./create-update-invoice.component.css"],
})
export class CreateUpdateInvoiceComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  isCreate = true;
  isExceedItemCountTreshold = false;
  showThresholdWarning = false;
  fetching = false;
  custom = false;

  customers: CustomerCurrency[] = [];
  customer_dispatch_details: CustomerDispatchDetails;
  dispatches: DispatchDetails[] = [];
  surharges: ExtraCharge[] = [];
  currencies: CurrencyDto[] = [];
  itemsByCurrency: SimplifiedItem[] = [];
  itemWrapper: ItemWrapper = {
    dispatchItems: [],
    surchargeItems: [],
    totalAmount: 0,
    totalAmountWithSurcharge: 0,
  };

  selected_customer: CustomerCurrency;
  selected_dispatches: any[] = [];

  invoice_item_count_threshold = 200;
  generateBy = 1;
  totalAmount = 0;

  months: string[] = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];
  startYear = 2024;
  years: string[] = Array.from({ length: 21 }, (_, i) =>
    (this.startYear + i).toString()
  );
  generateby: any[] = [
    { id: 1, name: "By Dispatch" },
    { id: 2, name: "By Bags" },
    { id: 3, name: "By Items" },
  ];

  selected_month = "";
  selected_year = "";
  custom_dispatch = "";

  invoiceDate: string;
  invoiceNo: string;
  invoiceNoDate: string;
  runningNo: string = "1";
  billTo: string;

  invoice_info: GenerateInvoice = {} as GenerateInvoice;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _invoiceService: InvoiceService,
    public _chibiService: ChibiService,
    public bsModalRef: BsModalRef,
    private datePipe: DatePipe,
    private _dispatchService: DispatchService,
    private _applicationSettingService: ApplicationSettingService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    const today = new Date();
    this.invoiceDate = this.datePipe.transform(today, "dd MMMM yyyy");
    this.invoiceNoDate = this.datePipe
      .transform(today, "MMM yyyy")
      .replace(" ", "")
      .toUpperCase();
    this.runningNo = this.runningNo.padStart(5, "0");
    this.invoiceNo = this.invoiceNoDate + "/___/" + this.runningNo;

    this.invoice_info.invoiceNo = this.invoiceNo;
    this.invoice_info.invoiceDate = this.invoiceDate;

    const currentDate = new Date();

    this.selected_month = currentDate.toLocaleDateString("en-US", {
      month: "short",
    });
    this.selected_year = currentDate.getFullYear().toString();

    this._applicationSettingService
      .getValueByName("InvoiceItemCountThreshold")
      .subscribe((result: any) => {
        this.invoice_item_count_threshold = +result.result;
      });
  }

  selectedCustom(event: any) {
    this.custom = event.target.value == "1" ? true : false;
  }

  getCustomerDispatchDetails() {
    this.dispatches = [];
    this.selected_dispatches = [];

    let monthYear = `${this.selected_month} ${this.selected_year}`;

    //Call API to get dispatches
    this._dispatchService
      .getDispatchesByCustomerMonthCurrency(
        this.selected_customer.code,
        monthYear,
        this.selected_customer.currency,
        this.custom
      )
      .subscribe((result: any) => {
        this.customer_dispatch_details = result.result;
        this.dispatches = this.customer_dispatch_details.details;
        this.invoice_info.billTo = this.customer_dispatch_details.address;
        this.generateBy = 1;
      });
  }

  selectedCustomer(event: any) {
    this.selected_customer = this.customers.find(
      (x: CustomerCurrency) => x.code === event.target.value
    );

    this.getCustomerDispatchDetails();
  }

  selectedDate(event: any) {
    this.selected_month = event.target.value;

    if (this.selectedCustomer != undefined) {
      this.getCustomerDispatchDetails();
    }
  }

  deleteSurcharge(index: number) {
    let surcharge_amount = 0;
    if (index > -1 && index < this.itemWrapper.surchargeItems.length) {
      surcharge_amount = this.itemWrapper.surchargeItems[index].amount;
      this.itemWrapper.surchargeItems.splice(index, 1);
    }
    this.itemWrapper.totalAmountWithSurcharge -= surcharge_amount;
  }

  validateAndCalculate(event: KeyboardEvent, index: number, input: string) {
    let surcharge = this.itemWrapper.surchargeItems[index];
    const inputChar = event.key;
    const currentInput = (event.target as HTMLInputElement).value;
    const newValue = currentInput + inputChar;

    switch (input) {
      case "weight":
        const pattern1 = /^\d+(\.\d{0,3})?$/;
        if (!pattern1.test(newValue)) {
          event.preventDefault();
        } else surcharge.weight = +newValue;
        break;
      case "ratePerKG":
        const pattern2 = /^\d+(\.\d{0,2})?$/;
        if (!pattern2.test(newValue)) {
          event.preventDefault();
        } else surcharge.rate = +newValue;
        break;
      case "unitPrice":
        const pattern3 = /^\d+(\.\d{0,2})?$/;
        if (!pattern3.test(newValue)) {
          event.preventDefault();
        } else surcharge.unitPrice = +newValue;
        break;
      case "quantity":
        const pattern4 = /^\d+$/;
        if (!pattern4.test(newValue)) {
          event.preventDefault();
        } else surcharge.quantity = +newValue;
        break;
      case "amount":
        const pattern5 = /^\d+(\.\d{0,2})?$/;
        if (!pattern5.test(newValue)) {
          event.preventDefault();
        } else surcharge.amount = +newValue;
        break;
    }

    if (input !== "amount") {
      let weight = +surcharge.weight;
      let ratePerKG = +surcharge.rate == 0 ? 1 : +surcharge.rate;
      let unitPrice = +surcharge.unitPrice;
      let quantity = +surcharge.quantity;

      surcharge.amount = +(weight * ratePerKG * unitPrice * quantity).toFixed(
        2
      );
    }

    let totalSurchargeAmount = 0;
    this.itemWrapper.surchargeItems.forEach((surcharge: SimplifiedItem) => {
      totalSurchargeAmount += surcharge.amount;
    });

    this.itemWrapper.totalAmountWithSurcharge =
      this.itemWrapper.totalAmount + totalSurchargeAmount;
  }

  addSurcharge() {
    this.itemWrapper.surchargeItems.push({
      dispatchNo: "",
      weight: 0,
      country: "",
      identifier: "",
      rate: 0,
      quantity: 0,
      unitPrice: 0,
      amount: 0,
      productCode: "",
      currency: "",
    });
  }

  onCheckboxChange() {
    let selected_item_count = 0;
    this.selected_dispatches = [];

    this.dispatches.forEach((dispatch: DispatchDetails) => {
      if (dispatch.selected) {
        selected_item_count += dispatch.itemCount;
        this.selected_dispatches.push(dispatch.name);
      }
    });

    if (selected_item_count > this.invoice_item_count_threshold)
      this.isExceedItemCountTreshold = true;
    else this.isExceedItemCountTreshold = false;

    if (this.generateBy === 3 && this.isExceedItemCountTreshold)
      this.showThresholdWarning = true;
    else this.showThresholdWarning = false;
  }

  selectedGenerateBy(event: any) {
    this.generateBy = +event.target.value;

    if (this.generateBy === 3 && this.isExceedItemCountTreshold)
      this.showThresholdWarning = true;
    else this.showThresholdWarning = false;
  }

  fetch() {
    this.fetching = true;

    let selected_dispatches = [];
    this.dispatches.forEach((dispatch: DispatchDetails) => {
      if (dispatch.selected) selected_dispatches.push(dispatch.name);
    });

    if (selected_dispatches.length > 0) {
      this._dispatchService
        .getItemsByCurrency(selected_dispatches, this.generateBy)
        .subscribe((result: any) => {
          this.itemWrapper = result.result;
          this.fetching = false;
        });
    }
  }

  save(): void {
    this.saving = true;

    if (this.itemWrapper.surchargeItems.length > 0) {
      this.invoice_info.extraCharges = [];
      this.itemWrapper.surchargeItems.forEach((surcharge: SimplifiedItem) => {
        this.invoice_info.extraCharges.push({
          description: surcharge.dispatchNo,
          weight: surcharge.weight,
          country: surcharge.country,
          ratePerKG: surcharge.rate,
          quantity: surcharge.quantity,
          unitPrice: surcharge.unitPrice,
          amount: surcharge.amount,
          currency: surcharge.currency,
        });
      });
    }
    else {
      this.invoice_info.extraCharges = [];
    }

    this.invoice_info.generateBy = this.generateBy;
    this.invoice_info.dispatches = this.selected_dispatches;
    this.invoice_info.customer = this.selected_customer.companyName;
    if (this.custom) {
      this.invoice_info.generateBy = 4;
      this.invoice_info.dispatches = [this.custom_dispatch];
    }

    let added_runningNo = +this.runningNo + 1;

    this._applicationSettingService
      .updateValueByName("InvoiceNo", added_runningNo.toString())
      .subscribe(() => {
        this._chibiService.createInvoiceQueue(this.invoice_info).subscribe(
          () => {
            this.notify.info(this.l("SavedSuccessfully"));
            this.bsModalRef.hide();
            this.onSave.emit();
          },
          () => {
            this.saving = false;
          }
        );
      });
  }
}
