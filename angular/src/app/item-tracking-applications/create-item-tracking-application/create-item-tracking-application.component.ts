import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { ItemTrackingApplicationService } from "../../../shared/service-proxies/item-tracking-applications/item-tracking-application.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { PostalService } from "@shared/service-proxies/postals/postal.service";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { ProductDDL } from "@shared/service-proxies/postals/model";
import { GroupedCustomerPostal } from "@shared/service-proxies/customer-postals/model";
import { HttpErrorResponse } from "@angular/common/http";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";

@Component({
  selector: "app-create-item-tracking-application",
  templateUrl: "./create-item-tracking-application.component.html",
  styleUrls: ["./create-item-tracking-application.component.css"],
})
export class CreateItemTrackingApplicationComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;

  customerCode: string;
  customerId: number;
  total: number;
  productCode: string;
  productDesc: string;
  postalCode: string;

  productDDL: ProductDDL[];
  groupedCustomerPostal: GroupedCustomerPostal[];

  isLoaded = false;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _itemtrackingapplicationService: ItemTrackingApplicationService,
    public _postalService: PostalService,
    public _customerPostalService: CustomerPostalService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._postalService.getProductDDL().subscribe((product: any) => {
        this.productDDL = product.result;
        this._customerPostalService.getGroupedCustomerPostal().subscribe((customerPostal: any) => {
            this.groupedCustomerPostal = customerPostal.result;
            this.isLoaded = true;
        });
    });
  }

  selectedCustomerPostal(event: any) {
    let split = event.target.value.split("+");
    let customer = split[1] == 'Any' ? undefined : this.groupedCustomerPostal.find(x => x.customerId == split[1]);

    this.customerCode = customer == undefined ? 'Any Account' : customer.customerCode;
    this.customerId = customer == undefined ? 0 : customer.customerId;
    this.postalCode = split[0];
  }

  selectedProduct(event: any) {
    let product = this.productDDL.find(x => x.productCode == event.target.value);

    this.productCode = product.productCode;
    this.productDesc = product.productDesc;
  }

  save(): void {
    this.saving = true;

    this._itemtrackingapplicationService
      .createTrackingApplication(
        this.customerCode,
        this.customerId,
        this.total,
        this.productCode,
        this.productDesc,
        this.postalCode
      )
      .subscribe(
        () => {
          this.notify.info(this.l("SavedSuccessfully"));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
          let cc: BsModalRef;
          cc = this._modalService.show(ErrorModalComponent, {
            class: "modal-lg",
            initialState: {
              title: "",
              errorMessage: error.message,
            },
          });
        },
        () => {
          this.saving = false;
        }
      );
  }
}
