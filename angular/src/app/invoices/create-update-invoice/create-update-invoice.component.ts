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
import { CustomerDto } from "@shared/service-proxies/customers/model";
import { CurrencyDto } from "@shared/service-proxies/currencies/model";
import { ApplicationSettingService } from "@shared/service-proxies/applicationsettings/applicationsetting.service";
import { DispatchDetails } from "@shared/service-proxies/dispatches/model";

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

  customers: CustomerDto[] = [];
  dispatches: DispatchDetails[] = [];
  surharges: ExtraCharge[] = [];
  currencies: CurrencyDto[] = [];

  selected_customer: CustomerDto;
  selected_dispatches: any[] = [];

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

  selected_month = "";
  selected_year = "";

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
  }

  selectedCustomer(event: any) {
    this.selected_customer = this.customers.find(
      (x: CustomerDto) => x.code === event.target.value
    );

    this.dispatches = [];
    this.selected_dispatches = [];

    let monthYear = `${this.selected_month} ${this.selected_year}`;

    //Call API to get dispatches
    this._dispatchService
      .getDispatchesByCustomerAndMonth(this.selected_customer.code, monthYear)
      .subscribe((result: any) => {
        this.dispatches = result.result;
        console.log(this.dispatches)
      });
  }

  selectedDate(event: any) {
    this.selected_month = event.target.value;

    if (this.selectedCustomer != undefined) {
      let monthYear = `${this.selected_month} ${this.selected_year}`;
      this._dispatchService
        .getDispatchesByCustomerAndMonth(this.selected_customer.code, monthYear)
        .subscribe((result: any) => {
          this.dispatches = result.result;
          console.log(this.dispatches)
        });
    }
  }

  deleteSurcharge(index: number) {
    if (index > -1 && index < this.surharges.length) {
      this.surharges.splice(index, 1);
    }
  }

  validateAndCalculate(event: KeyboardEvent, index: number, input: string) {
    let surcharge = this.surharges[index];
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
        } else surcharge.ratePerKG = +newValue;
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
    }

    let weight = +surcharge.weight;
    let ratePerKG = +surcharge.ratePerKG == 0 ? 1 : +surcharge.ratePerKG;
    let unitPrice = +surcharge.unitPrice;
    let quantity = +surcharge.quantity;

    surcharge.amount = +(weight * ratePerKG * unitPrice * quantity).toFixed(2);
  }

  addSurcharge() {
    this.surharges.push({
      description: "",
      weight: undefined,
      country: "",
      ratePerKG: undefined,
      quantity: undefined,
      unitPrice: undefined,
      amount: undefined,
      currency: undefined,
    });
  }

  onChange(event) {
    console.log("Selected Dispatches:", this.selected_dispatches);
  }

  save(): void {
    this.saving = true;

    this.invoice_info.dispatches = this.selected_dispatches;
    this.invoice_info.customer = this.selected_customer.companyName;
    this.invoice_info.extraCharges = this.surharges;

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
