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
import { ItemTrackingReviewDto } from "@shared/service-proxies/item-tracking-reviews/model";

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

  application: ItemTrackingApplicationDto;

  @Output() onSave = new EventEmitter<any>();

  ngOnInit(): void {
    this.suffix = this.application.postalCode;
  }

  constructor(
    injector: Injector,
    public _itemtrackingapplicationService: ItemTrackingApplicationService,
    public _itemtrackingreviewService: ItemTrackingReviewService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  save(): void {
    this.saving = true;

    let review: ItemTrackingReviewDto;
    review.applicationId = this.application.id;
    review.customerId = this.application.customerId;
    review.customerCode = this.application.customerCode;
    review.postalCode = this.application.postalCode;
    review.postalDesc = this.application.postalDesc;
    review.total = this.application.total;
    review.totalGiven = 0;
    review.productCode = this.application.productCode;
    review.status = "Approved";
    review.dateCreated = this.application.dateCreated;
    review.prefix = this.prefix;
    review.prefixNo = this.prefixNo;
    review.suffix = this.suffix;

    this._itemtrackingreviewService.create(review).subscribe(() => {
      this.notify.info(this.l("SavedSuccessfully"));
      this.bsModalRef.hide();
      this.onSave.emit();
    });
  }

  decline() {
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
    

    this._itemtrackingreviewService.create(review).subscribe(() => {
      this.notify.info(this.l("SavedSuccessfully"));
      this.bsModalRef.hide();
      this.onSave.emit();
    });
  }

  approve() {
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

    this._itemtrackingreviewService.create(review).subscribe(() => {
      this.notify.info(this.l("SavedSuccessfully"));
      this.bsModalRef.hide();
      this.onSave.emit();
    });
  }

  undo() {
    this._itemtrackingreviewService.undoReview(this.application.id).subscribe(() => {
      this.notify.info(this.l("SavedSuccessfully"));
      this.bsModalRef.hide();
      this.onSave.emit();
    });
  }

  invalid() {
    if (this.prefix.length == 0) return true;
    if (this.prefixNo.length == 0) return true;
    if (this.suffix.length == 0) return true;
    return false;
  }
}
