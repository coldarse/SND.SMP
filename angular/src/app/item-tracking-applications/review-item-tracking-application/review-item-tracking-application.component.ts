import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { ItemTrackingApplicationService } from "@shared/service-proxies/item-tracking-applications/item-tracking-application.service";
import { BsModalRef } from "ngx-bootstrap/modal";
import { ItemTrackingReviewService } from "@shared/service-proxies/item-tracking-reviews/item-tracking-review.service";
import { ItemTrackingApplicationDto } from "@shared/service-proxies/item-tracking-applications/model";
import {
  ItemTrackingReviewDto,
  ReviewAmount,
} from "@shared/service-proxies/item-tracking-reviews/model";

@Component({
  selector: "app-review-item-tracking-application",
  templateUrl: "./review-item-tracking-application.component.html",
  styleUrls: ["./review-item-tracking-application.component.css"],
})
export class ReviewItemTrackingApplicationComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;

  prefix = "";
  prefixNo = "";
  suffix = "";
  remark = "";

  prefixNoMaxLength = 7;
  prefixMaxLength = 2;

  application: ItemTrackingApplicationDto;

  amount: ReviewAmount;

  @Output() onSave = new EventEmitter<any>();

  ngOnInit(): void {
    this.suffix =
      this.application.postalCode == "CO" ? "" : this.application.postalCode;
    this._itemtrackingreviewService
      .getReviewAmount(this.application.id)
      .subscribe((data: any) => {
        this.amount = data.result;
      });

    this.prefixMaxLength = this.application.postalCode == "CO" ? 3 : 2;
    this.prefixNoMaxLength = 8 - (this.application.total - 1).toString().length;
  }

  constructor(
    injector: Injector,
    public _itemtrackingapplicationService: ItemTrackingApplicationService,
    public _itemtrackingreviewService: ItemTrackingReviewService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  decline() {
    this.saving = true;

    let review: ItemTrackingReviewDto = {
      applicationId: this.application.id,
      customerId: this.application.customerId,
      customerCode: this.application.customerCode,
      postalCode: this.application.postalCode,
      postalDesc: this.application.postalDesc,
      total: this.application.total,
      totalGiven: 0,
      productCode: this.application.productCode,
      status: "Declined",
      dateCreated: this.application.dateCreated,
      prefix: this.prefix,
      prefixNo: this.prefixNo.toString(),
      suffix: this.suffix,
      remark: this.remark,
    };

    this._itemtrackingreviewService.create(review).subscribe(
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

  approve() {
    this.saving = true;

    let review: ItemTrackingReviewDto = {
      applicationId: this.application.id,
      customerId: this.application.customerId,
      customerCode: this.application.customerCode,
      postalCode: this.application.postalCode,
      postalDesc: this.application.postalDesc,
      total: this.application.total,
      totalGiven: 0,
      productCode: this.application.productCode,
      status: "Approved",
      dateCreated: this.application.dateCreated,
      prefix: this.prefix,
      prefixNo: this.prefixNo.toString(),
      suffix: this.suffix,
      remark: this.remark,
    };

    this._itemtrackingreviewService.create(review).subscribe(
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

  undo() {
    this.saving = true;

    this._itemtrackingreviewService.undoReview(this.application.id).subscribe(
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

  invalid() {
    if (this.prefix.length == 0) return true;
    if (this.prefixNo.length == 0) return true;
    if (this.suffix.length == 0) {
      if(this.application.postalCode == 'CO') return true;
    }
    
    return false;
  }
}
