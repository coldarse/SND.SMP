import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { PostalService } from "@shared/service-proxies/postals/postal.service";

@Component({
  selector: "app-upload-retry",
  templateUrl: "./upload-retry.component.html",
  styleUrls: ["./upload-retry.component.css"],
})
export class UploadRetryComponent extends AppComponentBase implements OnInit {
  saving = false;
  formFile: any = undefined;

  postalItems: any[] = [];
  serviceItems: any[] = [];
  productItems: any[] = [];

  selectedCustomerCode = "";
  selectedServiceValue = "";
  selectedPostalValue = "";
  selectedPostalPrefix = "";
  selectedKGRateValue = "";
  selectedProductValue = "";

  filepath: string;
  dispatchNo: string;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _chibiService: ChibiService,
    private customerPostalService: CustomerPostalService,
    private postalService: PostalService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.customerPostalService
      .getCustomerPostalsByCustomerCode(this.selectedCustomerCode)
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

  handleUpload(event) {
    if (event.target.files.length > 0) {
      this.formFile = event.target.files[0];
    }
  }

  save(): void {
    this.saving = true;

    const form = new FormData();
    if (this.formFile != undefined) form.append("UploadFile.file", this.formFile);
    form.append("path", this.filepath);
    form.append("dispatchNo", this.dispatchNo);

    form.append("Details.DispatchNo", this.dispatchNo);
    form.append("Details.AccNo", this.selectedCustomerCode);
    form.append("Details.PostalCode", this.selectedPostalValue);
    form.append("Details.ServiceCode", this.selectedServiceValue);
    form.append("Details.ProductCode", this.selectedProductValue);
    form.append("Details.DateDispatch", '');
    form.append("Details.RateOptionId", this.selectedKGRateValue);

    this._chibiService.uploadRetryPreCheckFile(form).subscribe(
      () => {
        this.notify.info(this.l("UploadedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      },
      () => {
        this.saving = false;
      }
    );
  }
}
