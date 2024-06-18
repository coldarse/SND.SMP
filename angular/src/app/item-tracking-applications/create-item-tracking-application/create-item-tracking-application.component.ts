import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { ItemTrackingApplicationService } from "../../../shared/service-proxies/item-tracking-applications/item-tracking-application.service";
import { BsModalRef } from "ngx-bootstrap/modal";
import { PostalService } from "@shared/service-proxies/postals/postal.service";
import { CustomerPostalService } from "@shared/service-proxies/customer-postals/customer-postal.service";
import { ProductDDL } from "@shared/service-proxies/postals/model";
import { GroupedCustomerPostal } from "@shared/service-proxies/customer-postals/model";

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
    public bsModalRef: BsModalRef
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
        () => {
          this.saving = false;
        }
      );
  }
}
