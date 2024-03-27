import { Component, Injector, OnInit } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
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

  isAdmin = true;

  formFile: any;

  constructor(
    injector: Injector,
    private customerService: CustomerService,
    private customerPostalService: CustomerPostalService,
    private postalService: PostalService
  ) {
    super(injector);
    if (
      this.appSession.getShownLoginName().replace(".\\", "").includes("admin")
    )
      this.isAdmin = true;
    else this.isAdmin = false;
  }

  ngOnInit(): void {
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
    this.selectedCustomerValue = event.target.value;
    this.customerPostalService
      .getCustomerPostalsByAccountNo(this.selectedCustomerValue)
      .subscribe((postal: any) => {
        this.postalItems = postal.result;
      });
  }

  selectedPostal(event: any){
    this.selectedPostalPrefix = "";
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
}
