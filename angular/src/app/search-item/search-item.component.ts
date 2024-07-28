import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { ItemDetails, ItemInfo } from "@shared/service-proxies/items/model";
import { result } from "lodash-es";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs";
import {
  fadeInAnimation,
  fadeInCenterAnimation,
  fadeInFromAboveAnimation,
  fadeOutAnimation,
} from "../animations/animation";
import { ItemTrackingReviewService } from "@shared/service-proxies/item-tracking-reviews/item-tracking-review.service";

@Component({
  selector: "app-search-item",
  templateUrl: "./search-item.component.html",
  styleUrl: "./search-item.component.scss",
  animations: [
    fadeInAnimation,
    fadeInFromAboveAnimation,
    fadeInCenterAnimation,
    fadeOutAnimation
  ],
})
export class SearchItemComponent implements OnInit {
  trackingNo = "";

  itemInfo: ItemInfo = undefined;
  isFocused = true;
  searching = false;

  details = true;
  tracking = true;

  constructor(
    private _itemTrackingReviewService: ItemTrackingReviewService,
    private _modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.itemInfo = undefined;
  }

  onFocus(status: boolean){
    this.isFocused = status;
  }

  search() {
    this.itemInfo = undefined;
    this.searching = true;
    this._itemTrackingReviewService
      .getItem(this.trackingNo, this.details, this.tracking)
      .pipe(
        finalize(() => {
          this.searching = false;
        })
      )
      .subscribe(
        (elem: any) => {
          this.itemInfo = elem.result;
        },
        (error: HttpErrorResponse) => {
          //Handle error
          let cc: BsModalRef;
          cc = this._modalService.show(ErrorModalComponent, {
            class: "modal-lg",
            initialState: {
              title: "",
              errorMessage: error.message,
            },
          });
        }
      );
  }
}
