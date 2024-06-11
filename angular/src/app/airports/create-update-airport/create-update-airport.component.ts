import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { AirportDto } from '../../../shared/service-proxies/airports/model';
import { AirportService } from '../../../shared/service-proxies/airports/airport.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-airport',
  templateUrl: './create-update-airport.component.html',
  styleUrls: ['./create-update-airport.component.css']
})
export class CreateUpdateAirportComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  airport?: AirportDto = {} as AirportDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _airportService: AirportService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.airport.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.airport.id != undefined){
      this._airportService.update(this.airport).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        () => {
          this.saving = false;
        }
      );
    }
    else{
      this._airportService.create(this.airport).subscribe(
        () => {
          this.notify.info(this.l('SavedSuccessfully'));
          this.bsModalRef.hide();
          this.onSave.emit();
        },
        () => {
          this.saving = false;
        }
      );
    }

  }

}
