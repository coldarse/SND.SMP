import { Component, EventEmitter, Injector, OnInit, Output } from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { CustomerService } from "../../../shared/service-proxies/customers/customer.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { DatePipe } from "@angular/common";
import { GenerateInvoice } from "../../../shared/service-proxies/invoices/model";
import { ChibiService } from "../../../shared/service-proxies/chibis/chibis.service";

@Component({
  selector: "app-de-value",
  templateUrl: "./de-discount-value.component.html",
  styleUrls: ["./de-discount-value.component.css"],
})
export class DeDiscountValueComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  discount = 0;
  companyCode = "";
  dispatchNo = "";

  @Output() onSave = new EventEmitter<any>();

  invoice_info: GenerateInvoice = {} as GenerateInvoice;

  constructor(
    injector: Injector,
    public _customerService: CustomerService,
    public bsModalRef: BsModalRef,
    private datePipe: DatePipe,
    private _chibiService: ChibiService
  ) {
    super(injector);
  }
  ngOnInit(): void {
  }

  save() {
    this._customerService
      .getCompanyNameAndAddress(this.companyCode)
      .subscribe((data: any) => {
        this.invoice_info.customer = data.result.name;
        this.invoice_info.billTo = data.result.address;

        const today = new Date();
        let invoiceDate = this.datePipe.transform(today, "dd MMMM yyyy");
        let invoiceNoDate = this.datePipe
          .transform(today, "MMM yyyy")
          .replace(" ", "")
          .toUpperCase();
        let invoiceNo = invoiceNoDate + "/___/" + this.dispatchNo;

        this.invoice_info.invoiceNo = invoiceNo;
        this.invoice_info.invoiceDate = invoiceDate;
        this.invoice_info.generateBy = 3; // By Items

        this.invoice_info.extraCharges = [];
        this.invoice_info.dispatches = [];

        this.invoice_info.dispatches.push(this.dispatchNo);
        this.invoice_info.extraCharges.push({
          description: "",
          weight: 0,
          country: "",
          ratePerKG: 0,
          quantity: 0,
          unitPrice: 0,
          amount: this.discount,
          currency: "",
        });

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
