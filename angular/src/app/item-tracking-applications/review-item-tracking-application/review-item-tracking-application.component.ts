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

  application: ItemTrackingApplicationDto;

  @Output() onSave = new EventEmitter<any>();

  ngOnInit(): void {}

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

    // this._itemtrackingreviewService
    //   .createTrackingApplication()
    //   .subscribe(
    //     () => {
    //       this.notify.info(this.l("SavedSuccessfully"));
    //       this.bsModalRef.hide();
    //       this.onSave.emit();
    //     },
    //     () => {
    //       this.saving = false;
    //     }
    //   );
  }

  decline() {
    this.bsModalRef.hide();
  }

  approve() {}

  invalid() {
    if(this.prefix.length == 0) return true;
    if(this.prefixNo.length == 0) return true;
    if(this.suffix.length == 0) return true;
  }
}
