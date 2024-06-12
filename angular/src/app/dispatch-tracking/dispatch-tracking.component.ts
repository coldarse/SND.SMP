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
    this.keyword = this.keyword == '' ? undefined : this.keyword;
    this.country = this.country == '' ? undefined : this.country;

    this._dispatchService.getDispatchesForTracking(this.keyword, this.country).subscribe((data: any) => {
      this.dispatches = data.result.dispatches;
      this.countries = data.result.countries;
    });
  }

  selectCountry(event: any) {
    this.country = event.target.value == '' ? undefined : event.target.value;
    this.keyword = this.keyword == '' ? undefined : this.keyword;

    this._dispatchService.getDispatchesForTracking(this.keyword, this.country).subscribe((data: any) => {
      this.dispatches = data.result.dispatches;
      this.countries = data.result.countries;
    });
  }

  openCountry(index: any) {
    this.dispatches[index].open = !this.dispatches[index].open;
  }

  openBag(indexC: any, indexB: any) {
    this.dispatches[indexC].dispatchCountries[indexB].open = !this.dispatches[indexC].dispatchCountries[indexB].open;

    this.dispatches[indexC].dispatchCountries[indexB].dispatchBags.forEach(elem => {
      elem.select = this.dispatches[indexC].dispatchCountries[indexB].open;
    });
  }

  selectBag(indexC: any, indexB: any, indexS: any) {
    this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexS].select = !this.dispatches[indexC].dispatchCountries[indexB].dispatchBags[indexS].select;
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
