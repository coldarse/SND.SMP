import { Component, Injector, OnInit } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { AirportService } from "@shared/service-proxies/airports/airport.service";
import { AirportDto } from "@shared/service-proxies/airports/model";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { DispatchService } from "@shared/service-proxies/dispatches/dispatch.service";
import {
  DispatchBag,
  DispatchCountry,
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
    private _chibiService: ChibiService,
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

    if (this.dispatches[index].open == false) {
      this.dispatches[index].dispatchCountries.forEach(
        (elem: DispatchCountry) => {
          elem.open = false;
          elem.select = false;
        }
      );
    }
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

    if (selected) {
      let airport = this.airports.find((x) => x.country === countryCode);

      var bagStage = this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexZ].stages;

      var stageAirport = bagStage != undefined ? 
                                  this.airports.find((x) => x.name === bagStage.airport) != undefined ? 
                                            this.airports.find((x) => x.name === bagStage.airport).code : airport != undefined ? airport.code : "" : "";

      let stage: Stage = {
        stage1Desc: bagStage != undefined ? bagStage.stage1Desc : "",
        stage2Desc: bagStage != undefined ? bagStage.stage2Desc : "",
        stage3Desc: bagStage != undefined ? bagStage.stage3Desc : "",
        stage4Desc: bagStage != undefined ? bagStage.stage4Desc : "",
        stage5Desc: bagStage != undefined ? bagStage.stage5Desc : "",
        stage6Desc: bagStage != undefined ? bagStage.stage6Desc : "",
        stage7Desc: bagStage != undefined ? bagStage.stage7Desc : "",
        stage1DateTime: bagStage != undefined ? bagStage.stage1DateTime.substring(0, 19) : "",
        stage2DateTime: bagStage != undefined ? bagStage.stage2DateTime.substring(0, 19) : "",
        stage3DateTime: bagStage != undefined ? bagStage.stage3DateTime.substring(0, 19) : "",
        stage4DateTime: bagStage != undefined ? bagStage.stage4DateTime.substring(0, 19) : "",
        stage5DateTime: bagStage != undefined ? bagStage.stage5DateTime.substring(0, 19) : "",
        stage6DateTime: bagStage != undefined ? bagStage.stage6DateTime.substring(0, 19) : "",
        stage7DateTime: bagStage != undefined ? bagStage.stage7DateTime.substring(0, 19) : "",
        airport: stageAirport,
        airportDateTime: bagStage != undefined ? bagStage.airportDateTime : "",
        bagNo: bagStage != undefined ? bagStage.bagNo : "",
        dispatchNo: dispatchNo,
        countryCode: countryCode,
      };

      this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexZ].stages = stage;
    }
  }

  countrySelect(indexC: any, indexY: any) {
    this.dispatches[indexC].dispatchCountries[indexY].select =
      !this.dispatches[indexC].dispatchCountries[indexY].select;

    if (!this.dispatches[indexC].dispatchCountries[indexY].select)
      this.dispatches[indexC].dispatchCountries[indexY].open = false;

    let countryCode =
      this.dispatches[indexC].dispatchCountries[indexY].countryCode;
    let dispatchNo = this.dispatches[indexC].dispatch;
    let selected = this.dispatches[indexC].dispatchCountries[indexY].select;

    if (selected) {
      let airport = this.airports.find((x) => x.country === countryCode);

      var countryStage = this.dispatches[indexC].dispatchCountries[indexY].stages;

      var stageAirport = countryStage != undefined ? 
                                  this.airports.find((x) => x.name === countryStage.airport) != undefined ? 
                                            this.airports.find((x) => x.name === countryStage.airport).code : airport != undefined ? airport.code : "" : "";


      let stage: Stage = {
        stage1Desc: countryStage != undefined ? countryStage.stage1Desc : "",
        stage2Desc: countryStage != undefined ? countryStage.stage2Desc : "",
        stage3Desc: countryStage != undefined ? countryStage.stage3Desc : "",
        stage4Desc: countryStage != undefined ? countryStage.stage4Desc : "",
        stage5Desc: countryStage != undefined ? countryStage.stage5Desc : "",
        stage6Desc: countryStage != undefined ? countryStage.stage6Desc : "",
        stage7Desc: countryStage != undefined ? countryStage.stage7Desc : "",
        stage1DateTime: countryStage != undefined ? countryStage.stage1DateTime.substring(0, 19) : "",
        stage2DateTime: countryStage != undefined ? countryStage.stage2DateTime.substring(0, 19) : "",
        stage3DateTime: countryStage != undefined ? countryStage.stage3DateTime.substring(0, 19) : "",
        stage4DateTime: countryStage != undefined ? countryStage.stage4DateTime.substring(0, 19) : "",
        stage5DateTime: countryStage != undefined ? countryStage.stage5DateTime.substring(0, 19) : "",
        stage6DateTime: countryStage != undefined ? countryStage.stage6DateTime.substring(0, 19) : "",
        stage7DateTime: countryStage != undefined ? countryStage.stage7DateTime.substring(0, 19) : "",
        airport: stageAirport,
        airportDateTime: countryStage != undefined ? countryStage.airportDateTime : "",
        bagNo: countryStage != undefined ? countryStage.bagNo : "",
        dispatchNo: dispatchNo,
        countryCode: countryCode,
      };
      this.dispatches[indexC].dispatchCountries[indexY].stages = stage;
    }
  }

  inputStage(event: any, indexC: any, indexB: any, indexS: any, field: string) {
    let temp =
      indexS == ""
        ? this.dispatches[indexC].dispatchCountries[indexB].stages
        : this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexS]
            .stages;

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
  }

  submitItemTracking() {
    this.dispatches = this.dispatches.filter(di => di.dispatchCountries.some(dc => dc.select));
    this._chibiService.createTrackingFileForDispatches(this.dispatches).subscribe(() => {
      abp.notify.success(this.l('Successfully Queued For Update'));
      this.refresh();
    });
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
