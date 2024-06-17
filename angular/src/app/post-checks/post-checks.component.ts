import { Component, Injector, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { GetPostCheck } from "@shared/service-proxies/dispatches/model";
import { HttpErrorResponse } from "@angular/common/http";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import * as XLSX from "xlsx";
import { AppComponentBase } from "@shared/app-component-base";
import { finalize } from "rxjs";

@Component({
  selector: "app-post-checks",
  templateUrl: "./post-checks.component.html",
  styleUrl: "./post-checks.component.css",
})
export class PostChecksComponent extends AppComponentBase implements OnInit {
  constructor(
    injector: Injector,
    private _Activatedroute: ActivatedRoute,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService,
    private router: Router
  ) {
    super(injector);
  }

  fileUpload: any;
  sub: any;
  bags: any[] = [];

  bypassValue: number = 0;
  customer: string = "";
  dispatchNo: string = "";
  flight: string = "";
  eta: string = "";
  ata: string = "";
  accountNo: string = "";
  mawb: string = "";

  preCheckNoOfBag: number = 0;
  postCheckNoOfBag: number = 0;
  preCheckWeight: number = 0;
  postCheckWeight: number = 0;

  postchecks: GetPostCheck = undefined;

  isLoading = false;

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

  handleUpload(event) {
    if (event.target.files.length > 0) {
      this.fileUpload = event.target.files[0];
    } else {
      this.fileUpload = undefined;
    }
  }

  calculate(event: any, index: any) {
    let bag = this.postchecks.bags.find((x) => x.id === index.id);
    bag.weightPost = +event.target.value;
    bag.weightVariance = +(bag.weightPre - bag.weightPost).toFixed(3);

    let totalBagsPostChecked = 0;
    let totalWeightPostChecked = 0;

    this.postchecks.bags.forEach((element) => {
      if (element.weightPost != null && element.weightPost != 0) {
        totalBagsPostChecked += 1;
        totalWeightPostChecked += +element.weightPost;
      }
    });

    this.postchecks.postCheckWeight = totalWeightPostChecked;
    this.postchecks.postCheckNoOfBag = totalBagsPostChecked;
  }

  backToHome() {
    this.router.navigate(["/app/home"]);
  }

  downloadTemplate() {
    let postCheckHeaders: string[] = [
      "Postal",
      "Dispatch Date",
      "Service",
      "Product Code",
      "Bag No",
      "Country",
      "Weight",
      "Quantity",
      "Seal Number",
      "Dispatch Name",
    ];
    let postCheckTemplate: string[][] = [postCheckHeaders, []];

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(postCheckTemplate);
    ws['!cols'] = this.fitToColumn(postCheckHeaders);
    XLSX.utils.book_append_sheet(wb, ws, "Sheet 1");
    XLSX.writeFile(wb, `PostCheckUpload.xlsx`);
  }

  fitToColumn(arrayOfArray: any[]) {
    let widths = [];
    arrayOfArray.forEach((elem) => {
      widths.push({
        wch: Math.max(elem.length) + 1
      });
    })

    return widths;
  }

  SubmitPostCheck() {
    this._dispatchService.savePostCheck(this.postchecks).subscribe(
      (result: any) => {
        if (result.result) {
          this.notify.info(this.l("Post Check Completed"));
          this.router.navigate(["/app/dispatches"]);
        } else this.notify.error(this.l("Error Post Check"));
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

  BypassPostCheck() {
    this.isLoading = true;
    this._dispatchService
      .bypassPostCheck(this.dispatchNo, this.bypassValue)
      .pipe(
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe(
        (result: any) => {
          if (result.result) {
            this.notify.info(this.l("ByPassed Post Check"));
            this.router.navigate(["/app/dispatches"]);
          } else this.notify.error(this.l("Failed to ByPass Post Check"));
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

  UploadPostCheck() {
    const form = new FormData();
    form.append("file", this.fileUpload);
    form.append("dispatchNo", this.dispatchNo);

    this._dispatchService.uploadPostCheck(form).subscribe(
      (result: any) => {
        if (result.result) {
          this.notify.info(this.l("UploadedSuccessfully"));
          this.router.navigate(["/app/dispatches"]);
        } else this.notify.error(this.l("UploadFailed"));
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
