import { Component, Injector, OnInit } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { CustomerService } from "@shared/service-proxies/customers/customer.service";
import { PostalService } from "@shared/service-proxies/postals/postal.service";

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
    private customerService: CustomerService,
    private customerPostalService: CustomerPostalService,
    private postalService: PostalService,
    private chibiService: ChibiService
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

    const customer = this.customerItems.find(x => x.id === this.selectedCustomerValue);

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

      this.ngOnInit();
    });
  }
}
