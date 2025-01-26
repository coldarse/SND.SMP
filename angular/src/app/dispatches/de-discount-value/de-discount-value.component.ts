import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { CustomerService } from "../../../shared/service-proxies/customers/customer.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { DatePipe } from "@angular/common";
import { GenerateInvoice } from "../../../shared/service-proxies/invoices/model";
import { ChibiService } from "../../../shared/service-proxies/chibis/chibis.service";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { HttpErrorResponse, HttpResponse } from "@angular/common/http";

import * as XLSX from "xlsx";
import { ApplicationSettingService } from "@shared/service-proxies/applicationsettings/applicationsetting.service";

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
  mawb = "";
  runningNo: string = "1";

  isExcel = false;

  isManifest = false;

  @Output() onSave = new EventEmitter<any>();
  @Output() onSubmit = new EventEmitter<any[]>();

  invoice_info: GenerateInvoice = {} as GenerateInvoice;

  constructor(
    injector: Injector,
    public _customerService: CustomerService,
    public bsModalRef: BsModalRef,
    private datePipe: DatePipe,
    private _chibiService: ChibiService,
    private _dispatchService: DispatchService,
    private _applicationSettingService: ApplicationSettingService
  ) {
    super(injector);
  }
  ngOnInit(): void {}

  save() {
    if (this.isManifest) {
      this.bsModalRef.hide();
      this.onSubmit.emit([this.dispatchNo, this.mawb, this.discount]);
      
    } else {
      if (this.isExcel) {
        this._dispatchService
          .getCommercialInvoiceExcelItems(
            this.dispatchNo,
            this.discount.toString()
          )
          .subscribe((data: any) => {
            let items = data.result;

            // Function to transform headers
            const formatHeader = (header: string): string => {
              return header
                .replace(/([a-z])([A-Z])/g, "$1 $2") // Add space before capital letters
                .replace(/^./, (str) => str.toUpperCase()); // Capitalize the first letter
            };

            // Format headers to be human-readable
            const headers =
              items.length > 0 ? Object.keys(items[0]).map(formatHeader) : [];

            // Convert the array of objects to a worksheet without auto-generating headers
            const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet([], {
              skipHeader: true,
            }); // Start with an empty sheet

            // Manually add the formatted headers
            XLSX.utils.sheet_add_aoa(ws, [headers], { origin: "A1" });

            // Add the data rows
            XLSX.utils.sheet_add_json(ws, items, {
              origin: "A2",
              skipHeader: true,
            }); // Add data starting below the headers

            // Auto-fit columns based on headers and data length
            ws["!cols"] = this.fitToColumn(headers, items);

            // Create a new workbook and append the worksheet
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, "Sheet 1");

            // Write the workbook to a file
            XLSX.writeFile(wb, `CommercialInvoice_${this.dispatchNo}.xlsx`);

            this.bsModalRef.hide();
            this.onSave.emit();
          });
      } else {
        this.saving = true;

        this._customerService
          .getCompanyNameAndAddress(this.companyCode)
          .subscribe((data: any) => {
            this.invoice_info.customer = data.result.name;
            this.invoice_info.billTo = data.result.address;

            const today = new Date();
            let invoiceDate = this.datePipe.transform(today, "dd MMMM yyyy");
            let invoiceNoDate = this.datePipe
              .transform(today, "yyyyMM")
              .replace(" ", "")
              .toUpperCase();
            this.runningNo = this.runningNo.padStart(5, "0");
            let invoiceNo = `SMI${invoiceNoDate}${this.runningNo}`;

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

            let added_runningNo = +this.runningNo + 1;

            this._applicationSettingService
              .updateValueByName(
                "CommercialInvoiceNo",
                added_runningNo.toString()
              )
              .subscribe(() => {
                this._chibiService
                  .createInvoiceQueue(this.invoice_info)
                  .subscribe(
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
          });
      }
    }
  }

  fitToColumn(headers: string[], data: any[] = []): { width: number }[] {
    return headers.map((header) => {
      const maxHeaderWidth = header.length;
      const maxDataWidth = Math.max(
        ...data.map((item) =>
          item[header] ? item[header].toString().length : 0
        ),
        maxHeaderWidth
      );
      return { width: maxDataWidth + 2 }; // Add padding
    });
  }
}
