import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-post-checks",
  templateUrl: "./post-checks.component.html",
  styleUrl: "./post-checks.component.css",
})
export class PostChecksComponent implements OnInit {
  bypassValue: string = "";
  fileUpload: any;

  customer: string = "Signature Mail";
  dispatchNo: string = "KGOMTY0048";
  flight: string = "";
  eta: string = "";
  ata: string = "";

  accountNo: string = "IPS";
  mawb: string = ".";
  preCheckNoOfBag: number = 21;
  postCheckNoOfBag: number = 21;
  preCheckWeight: number = 377.83;
  postCheckWeight: number = 380.8;

  bags: any[] = [
    {
      bagno: "81AU240412001",
      quantity: 0,
      totalWeight: 17.38,
      actualWeight: 0,
      weightVariance: 0,
    },
    {
      bagno: "81AU240412002",
      quantity: 0,
      totalWeight: 21.23,
      actualWeight: 0,
      weightVariance: 0,
    },
    {
      bagno: "81AU240412003",
      quantity: 0,
      totalWeight: 18.69,
      actualWeight: 0,
      weightVariance: 0,
    },
  ];

  ngOnInit(): void {}

  calculate(index: number) {
    let bag = this.bags.at(index);
    bag.weightVariance = bag.totalWeight - bag.actualWeight;
  }

  diff = (a, b) => {
    return Math.abs(a - b);
  };
}
