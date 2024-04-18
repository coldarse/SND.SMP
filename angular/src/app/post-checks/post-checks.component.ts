import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute } from "@angular/router";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import { GetPostCheck } from "@shared/service-proxies/dispatches/model";

@Component({
  selector: "app-post-checks",
  templateUrl: "./post-checks.component.html",
  styleUrl: "./post-checks.component.css",
})
export class PostChecksComponent implements OnInit {

  constructor(
    private _Activatedroute: ActivatedRoute,
    private _dispatchService: DispatchService 
  ){

  }
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
  postchecks: GetPostCheck;

  bags: any[] = []

  ngOnInit(): void {
    this.sub = this._Activatedroute.paramMap.subscribe((params) => {
      this.dispatchNo = params.get('dispatchNo');
      this._dispatchService.getPostCheck(this.dispatchNo).subscribe((result: any) => {
        this.postchecks = result.result;
      });
    });
      
  }

  calculate(index: number) {
    let bag = this.bags.at(index);
    bag.weightVariance = bag.totalWeight - bag.actualWeight;
  }

  diff = (a, b) => {
    return Math.abs(a - b);
  };
}
