import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { ItemService } from "@shared/service-proxies/items/item.service";
import { ItemDetails } from "@shared/service-proxies/items/model";
import { result } from "lodash-es";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs";
import {
  fadeInAnimation,
  fadeInCenterAnimation,
  fadeInFromAboveAnimation,
  fadeOutAnimation,
} from "../animations/animation";

@Component({
  selector: "app-search-item",
  templateUrl: "./search-item.component.html",
  styleUrl: "./search-item.component.css",
  animations: [
    fadeInAnimation,
    fadeInFromAboveAnimation,
    fadeInCenterAnimation,
    fadeOutAnimation
  ],
})
export class SearchItemComponent implements OnInit {
  trackingNo = "";

  itemDetails: ItemDetails = undefined;

  searching = false;

  constructor(
    private _itemService: ItemService,
    private _modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.itemDetails = undefined;
  }

  search() {
    this.itemDetails = undefined;
    this.searching = true;
    this._itemService
      .getItem(this.trackingNo)
      .pipe(
        finalize(() => {
          this.searching = false;
        })
      )
      .subscribe(
        (elem: any) => {
          this.itemDetails = elem.result;
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
