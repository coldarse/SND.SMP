import { Component, EventEmitter, Injector, OnInit, Output } from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";
import { AirportDto } from "@shared/service-proxies/airports/model";

@Component({
  selector: "app-stage-4-airport",
  templateUrl: "./stage-4-airport.component.html",
  styleUrls: ["./stage-4-airport.component.css"],
})
export class Stage4AirportComponent
  extends AppComponentBase
  implements OnInit {

    countries: string[] = [];
    airports: AirportDto[] = [];

    countriesWithAirports = [];

    disableUpdate = false;

    @Output() closeModal = new EventEmitter<any[]>();

    ngOnInit(): void {
        this.countries.forEach((country: string) => {
            let temp_airports = this.airports.filter(x => x.country === country);

            this.countriesWithAirports.push({ 
                country: country, 
                airports: temp_airports,
                airport: temp_airports.length == 0 ? '' : temp_airports[0].name
            });
        });
    }

    selectedAirport(event: any, index: number){
        this.countriesWithAirports[index].airport = event.target.value;

        let temp_airports = this.countriesWithAirports.filter(x => x.airport === '');

        if(temp_airports.length == this.countriesWithAirports.length) this.disableUpdate = true;
        else this.disableUpdate = false;
    }

    constructor(
        injector: Injector,
        public bsModalRef: BsModalRef,
    ){
        super(injector);
    }

    okay(): void {
        this.bsModalRef.hide();
        this.closeModal.emit(this.countriesWithAirports);
    }
    
  }
