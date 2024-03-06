import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { PostalCountryDto } from '../../../shared/service-proxies/postalcountries/model';
import { PostalCountryService } from '../../../shared/service-proxies/postalcountries/postalcountry.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-postalcountry',
  templateUrl: './create-update-postalcountry.component.html',
  styleUrls: ['./create-update-postalcountry.component.css']
})
export class CreateUpdatePostalCountryComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  postalcountry?: PostalCountryDto = {} as PostalCountryDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _postalcountryService: PostalCountryService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.postalcountry.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.postalcountry.id != undefined){
      this._postalcountryService.update(this.postalcountry).subscribe(
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
      this._postalcountryService.create(this.postalcountry).subscribe(
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
