import { HttpErrorResponse } from "@angular/common/http";
import { Component, Injector, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { AppComponentBase } from "@shared/app-component-base";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";
import { PostalService } from "@shared/service-proxies/postals/postal.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import * as XLSX from "xlsx";

@Component({
  selector: "app-pre-alerts",
  templateUrl: "./pre-alerts.component.html",
  styleUrls: ["./pre-alerts.component.css"],
})
export class PreAlertComponent extends AppComponentBase implements OnInit {
  customerItems: any[] = [];
  postalItems: any[] = [];
  serviceItems: any[] = [];
  productItems: any[] = [];

  selectedCustomerValue = 0;
  selectedPostalValue = "";
  selectedServiceValue = "";
  selectedProductValue = "";
  selectedKGRateValue = "";
  selectedPostalPrefix = "";
  selectedDispatchDate = new Date().toISOString().substring(0, 10);

  isAdmin = true;
  isSaving = false;

  formFile: any = undefined;

  dispatchNo = "";

  constructor(
    injector: Injector,
    private router: Router,
    private customerService: CustomerService,
    private customerPostalService: CustomerPostalService,
    private postalService: PostalService,
    private chibiService: ChibiService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
    if (
      this.appSession.getShownLoginName().replace(".\\", "").includes("admin")
    )
      this.isAdmin = true;
    else this.isAdmin = false;
  }

  ngOnInit(): void {
    this.resetValues();
    if (this.isAdmin) {
      this.customerService.getAllCustomers().subscribe((customer: any) => {
        this.customerItems = customer.result;
      });
    } else {
      this.customerService.getAllCustomers().subscribe((customer: any) => {
        this.customerItems = customer.result;
        this.selectedCustomerValue = this.appSession.getCustomerId();
        this.customerPostalService
          .getCustomerPostalsByAccountNo(this.selectedCustomerValue)
          .subscribe((postal: any) => {
            this.postalItems = postal.result;
          });
      });
    }
  }

  selectedCustomer(event: any) {
    this.postalItems = [];
    this.serviceItems = [];
    this.productItems = [];
    this.selectedPostalValue = "";
    this.selectedServiceValue = "";
    this.selectedProductValue = "";
    this.selectedKGRateValue = "";
    this.selectedCustomerValue = event.target.value;
    this.customerPostalService
      .getCustomerPostalsByAccountNo(this.selectedCustomerValue)
      .subscribe((postal: any) => {
        this.postalItems = postal.result;
      });
  }

  selectedPostal(event: any) {
    this.selectedPostalPrefix = "";
    this.selectedKGRateValue = "";
    this.serviceItems = [];
    this.productItems = [];
    this.selectedPostalValue = event.target.value;
    this.postalService
      .getServicesByPostal(this.selectedPostalValue)
      .subscribe((service: any) => {
        this.serviceItems = service.result;
      });
    this.selectedPostalPrefix = this.selectedPostalValue.substring(0, 2);
  }

  selectedService(event: any) {
    this.productItems = [];
    this.selectedServiceValue = event.target.value;
    this.postalService
      .getProductsByPostalAndService(
        this.selectedPostalValue,
        this.selectedServiceValue
      )
      .subscribe((product: any) => {
        this.productItems = product.result;
      });
  }

  selectedProduct(event: any) {
    this.selectedProductValue = event.target.value;
  }

  selectedKGRate(event: any) {
    this.selectedKGRateValue = event.target.value;
  }

  handleUpload(event: any) {
    if (event.target.files.length > 0) {
      this.formFile = event.target.files[0];
    }
  }

  resetValues() {
    this.customerItems = [];
    this.postalItems = [];
    this.serviceItems = [];
    this.productItems = [];
    // this.formFile = undefined;
    this.dispatchNo = "";
    this.selectedCustomerValue = 0;
    this.selectedPostalValue = "";
    this.selectedServiceValue = "";
    this.selectedProductValue = "";
    this.selectedKGRateValue = "";
    this.selectedPostalPrefix = "";
    this.selectedDispatchDate = new Date().toISOString().substring(0, 10);
  }

  checkInvalid() {
    if (
      this.selectedCustomerValue === 0 ||
      this.selectedPostalValue === "" ||
      this.selectedServiceValue === "" ||
      this.selectedProductValue === "" ||
      this.dispatchNo === "" ||
      this.formFile === undefined
    )
      return true;
  }

  upload() {
    this.isSaving = true;

    const customer = this.customerItems.find(x => +x.id === +this.selectedCustomerValue);

    const form = new FormData();
    form.append("UploadFile.file", this.formFile);
    form.append("Details.DispatchNo", this.dispatchNo);
    form.append("Details.AccNo", customer.code);
    form.append("Details.PostalCode", this.selectedPostalValue);
    form.append("Details.ServiceCode", this.selectedServiceValue);
    form.append("Details.ProductCode", this.selectedProductValue);
    form.append("Details.DateDispatch", this.selectedDispatchDate);
    form.append("Details.RateOptionId", this.selectedKGRateValue);

    this.chibiService.uploadPreCheckFile(form).subscribe(() => {
      this.notify.info(this.l("UploadedSuccessfully"));
      this.isSaving = false;


      this.router.navigate(['/app/home']);
    },
    (error: HttpErrorResponse) => {
      this.isSaving = false;
      //Handle error
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
    });
  }

  downloadTemplate(){
    let preCheckHeaders: string[] = [
      "Postal",
      "Dispatch Date",
      "Service",
      "Product Code",
      "Bag No",
      "Country",
      "Weight",
      "Tracking Number",
      "Seal Number",
      "Dispatch Name",
      "Item Value",
      "Item Desc",
      "Recp Name",
      "Tel No",
      "Email",
      "Address",
      "Postcode",
      "City",
      "Address Line 2",
      "Address No",
      "Identity No",
      "Identity Type",
      "State",
      "Length",
      "Width",
      "Height",
      "Tax Payment Method",
      "HS Code",
      "Qty"
    ];
    
    let preCheckTemplate: string[][] = [preCheckHeaders, []];

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(preCheckTemplate);
    ws['!cols'] = this.fitToColumn(preCheckHeaders);
    XLSX.utils.book_append_sheet(wb, ws, "Sheet 1");
    XLSX.writeFile(wb, `PreCheckUpload.xlsx`);
  }

  fitToColumn(arrayOfArray: any[]) {
    let widths = [];
    arrayOfArray.forEach((elem) => {
      widths.push({
        wch: Math.max(elem.length) + 1
      });
    })

    return widths;
  }
}
