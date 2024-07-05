import { Component, Injector, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { AirportService } from "@shared/service-proxies/airports/airport.service";
import { AirportDto } from "@shared/service-proxies/airports/model";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import {
  DispatchInfo,
  DispatchTracking,
  Stage,
} from "@shared/service-proxies/dispatches/model";

@Component({
  selector: "app-dispatch-tracking",
  templateUrl: "./dispatch-tracking.component.html",
  styleUrl: "./dispatch-tracking.component.css",
})
export class DispatchTrackingComponent
  extends PagedListingComponentBase<any>
  implements OnInit
{
  dispatches: DispatchInfo[] = [];

  countries: string[] = [];

  airports: AirportDto[] = [];

  country = "";

  keyword = "";

  constructor(
    private _dispatchService: DispatchService,
    private _airportService: AirportService,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._dispatchService.getDispatchesForTracking().subscribe((data: any) => {
      this.dispatches = data.result.dispatches;
      this.countries = data.result.countries;
    });
    this._airportService.getAirportList().subscribe((data: any) => {
      this.airports = data.result;
    });
  }

  search() {
    this.keyword = this.keyword == "" ? undefined : this.keyword;
    this.country = this.country == "" ? undefined : this.country;

    this._dispatchService
      .getDispatchesForTracking(this.keyword, this.country)
      .subscribe((data: any) => {
        this.dispatches = data.result.dispatches;
        this.countries = data.result.countries;
      });
  }

  selectCountry(event: any) {
    this.country = event.target.value == "" ? undefined : event.target.value;
    this.keyword = this.keyword == "" ? undefined : this.keyword;

    this._dispatchService
      .getDispatchesForTracking(this.keyword, this.country)
      .subscribe((data: any) => {
        this.dispatches = data.result.dispatches;
        this.countries = data.result.countries;
      });
  }

  openCountry(index: any) {
    this.dispatches[index].open = !this.dispatches[index].open;
  }

  openCloseBag(indexC: any, indexB: any) {
    this.dispatches[indexC].dispatchCountries[indexB].open =
      !this.dispatches[indexC].dispatchCountries[indexB].open;

    this.dispatches[indexC].dispatchCountries[indexB].dispatchBags.forEach(
      (elem) => {
        elem.select = this.dispatches[indexC].dispatchCountries[indexB].open;
      }
    );
  }

  selectBag(indexC: any, indexB: any, indexS: any) {
    this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[
      indexS
    ].select =
      !this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexS]
        .select;
  }

  custom(indexC: any, indexB: any, indexZ: any) {
    this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[
      indexZ
    ].custom =
      !this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexZ]
        .custom;

    let countryCode =
      this.dispatches[indexC].dispatchCountries[indexB].countryCode;
    let dispatchNo = this.dispatches[indexC].dispatch;
    let selected =
      this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexZ]
        .custom;
    let bagNo =
      this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexZ]
        .bagNo;

    if (selected) {
      let airport = this.airports.find((x) => x.country === countryCode);

      let stage: Stage = {
        stage1Desc: "",
        stage2Desc: "",
        stage3Desc: "",
        stage4Desc: "",
        stage5Desc: "",
        stage6Desc: "",
        stage7Desc: "",
        stage1DateTime: "",
        stage2DateTime: "",
        stage3DateTime: "",
        stage4DateTime: "",
        stage5DateTime: "",
        stage6DateTime: "",
        stage7DateTime: "",
        airport: airport != undefined ? airport.code : "",
        airportDateTime: "",
        bagNo: bagNo,
        dispatchNo: dispatchNo,
        countryCode: countryCode,
      };
      this.dispatches[indexC].stages.push(stage);
    } else {
      for (let i = this.dispatches[indexC].stages.length - 1; i >= 0; i--) {
        if (this.dispatches[indexC].stages[i].bagNo === bagNo) {
          this.dispatches[indexC].stages.splice(i, 1);
        }
      }
    }
    console.log(this.dispatches[indexC].stages);
  }

  countrySelect(indexC: any, indexY: any) {
    this.dispatches[indexC].dispatchCountries[indexY].select =
      !this.dispatches[indexC].dispatchCountries[indexY].select;

    let countryCode =
      this.dispatches[indexC].dispatchCountries[indexY].countryCode;
    let dispatchNo = this.dispatches[indexC].dispatch;
    let selected = this.dispatches[indexC].dispatchCountries[indexY].select;

    if (selected) {
      let airport = this.airports.find((x) => x.country === countryCode);

      let stage: Stage = {
        stage1Desc: "",
        stage2Desc: "",
        stage3Desc: "",
        stage4Desc: "",
        stage5Desc: "",
        stage6Desc: "",
        stage7Desc: "",
        stage1DateTime: "",
        stage2DateTime: "",
        stage3DateTime: "",
        stage4DateTime: "",
        stage5DateTime: "",
        stage6DateTime: "",
        stage7DateTime: "",
        airport: airport != undefined ? airport.code : "",
        airportDateTime: "",
        bagNo: "",
        dispatchNo: dispatchNo,
        countryCode: countryCode,
      };
      this.dispatches[indexC].stages.push(stage);
    } else {
      for (let i = this.dispatches[indexC].stages.length - 1; i >= 0; i--) {
        if (
          this.dispatches[indexC].stages[i].dispatchNo === dispatchNo &&
          this.dispatches[indexC].stages[i].countryCode === countryCode
        ) {
          this.dispatches[indexC].stages.splice(i, 1);
        }
      }
    }
    console.log(this.dispatches[indexC].stages);
  }

  inputStage(event: any, indexC: any, bagNo: string, field: string) {
    let temp = this.dispatches[indexC].stages.find((x) => x.bagNo === bagNo);

    if (temp != undefined) {
      switch (field) {
        case "stage1": {
          temp.stage1Desc = event.target.value;
          break;
        }
        case "stage2": {
          temp.stage2Desc = event.target.value;
          break;
        }
        case "stage3": {
          temp.stage3Desc = event.target.value;
          break;
        }
        case "stage4": {
          temp.stage4Desc = event.target.value;
          break;
        }
        case "stage5": {
          temp.stage5Desc = event.target.value;
          break;
        }
        case "stage6": {
          temp.stage6Desc = event.target.value;
          break;
        }
        case "stage7": {
          temp.stage7Desc = event.target.value;
          break;
        }
        case "airport": {
          temp.airport = event.target.value;
          break;
        }
        case "airportTime": {
          temp.airportDateTime = event.target.value;
          break;
        }
        case "time1": {
          temp.stage1DateTime = event.target.value;
          break;
        }
        case "time2": {
          temp.stage2DateTime = event.target.value;
          break;
        }
        case "time3": {
          temp.stage3DateTime = event.target.value;
          break;
        }
        case "time4": {
          temp.stage4DateTime = event.target.value;
          break;
        }
        case "time5": {
          temp.stage5DateTime = event.target.value;
          break;
        }
        case "time6": {
          temp.stage6DateTime = event.target.value;
          break;
        }
        case "time7": {
          temp.stage7DateTime = event.target.value;
          break;
        }
      }
    }

    console.log(this.dispatches[indexC].stages);
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    throw new Error("Method not implemented.");
  }
  protected delete(entity: any): void {
    throw new Error("Method not implemented.");
  }
}
