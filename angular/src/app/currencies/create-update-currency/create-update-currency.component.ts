import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { CurrencyDto } from '../../../shared/service-proxies/currencies/model';
import { CurrencyService } from '../../../shared/service-proxies/currencies/currency.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-currency',
  templateUrl: './create-update-currency.component.html',
  styleUrls: ['./create-update-currency.component.css']
})
export class CreateUpdateCurrencyComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  currency?: CurrencyDto = {} as CurrencyDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _currencyService: CurrencyService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.currency.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.currency.id != undefined){
      this._currencyService.update(this.currency).subscribe(
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
      this._currencyService.create(this.currency).subscribe(
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
