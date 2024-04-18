import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { GetPostCheck } from "@shared/service-proxies/dispatches/model";
import { HttpErrorResponse } from "@angular/common/http";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import * as XLSX from "xlsx";

@Component({
  selector: "app-post-checks",
  templateUrl: "./post-checks.component.html",
  styleUrl: "./post-checks.component.css",
})
export class PostChecksComponent implements OnInit {
  constructor(
    private _Activatedroute: ActivatedRoute,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService,
    private router: Router
  ) {}
  bypassValue: string = "";
  fileUpload: any;

  customer: string = "Signature Mail";
  dispatchNo: string = "";
  flight: string = "";
  eta: string = "";
  ata: string = "";

  accountNo: string = "IPS";
  mawb: string = ".";
  preCheckNoOfBag: number = 21;
  postCheckNoOfBag: number = 21;
  preCheckWeight: number = 377.83;
  postCheckWeight: number = 380.8;

  sub: any;
  postchecks: GetPostCheck = undefined;

  bags: any[] = [];

  ngOnInit(): void {
    this.postchecks = undefined;
    this.sub = this._Activatedroute.paramMap.subscribe((params) => {
      this.dispatchNo = params.get("dispatchNo");
      this._dispatchService.getPostCheck(this.dispatchNo).subscribe(
        (result: any) => {
          this.postchecks = result.result;
        },
        (error: HttpErrorResponse) => {
          this.postchecks = {} as GetPostCheck;
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
    });
  }

  calculate(event: any, index: any) {
    let bag = this.postchecks.bags.find((x) => x.id === index.id);
    bag.weightPost = +event.target.value;
    bag.weightVariance = bag.weightPre - bag.weightPost;
  }

  backToHome() {
    this.router.navigate(["/app/home"]);
  }

  downloadTemplate() {

    let postCheckHeaders:string[] = ["Postal","Dispatch Date","Service","Product Code","Bag No","Country","Weight","Quantity","Seal Number","Dispatch Name"];
    let postCheckTemplate:string[][] = [postCheckHeaders,[]];

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(postCheckTemplate);
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet 1');
    XLSX.writeFile(wb, `PostCheckUpload.xlsx`);
  }
}
