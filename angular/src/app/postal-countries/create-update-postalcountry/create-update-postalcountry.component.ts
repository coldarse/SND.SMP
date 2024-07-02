import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { PostalCountryDto } from '../../../shared/service-proxies/postal-countries/model';
import { PostalCountryService } from '../../../shared/service-proxies/postal-countries/postal-country.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { PostalService } from '@shared/service-proxies/postals/postal.service';
import { PostalDDL } from '@shared/service-proxies/postals/model';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

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
  isLoaded = false;

  postalDDL: PostalDDL[] = [];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _postalcountryService: PostalCountryService,
    public _postalService: PostalService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._postalService.getPostalDDL().subscribe((data: any) => {
      this.postalDDL = data.result;
      if(this.postalcountry.id != undefined){
        this.isCreate = false;
      }
      this.isLoaded = true;
    });
  }

  selectedPostal(event: any) {
    this.postalcountry.postalCode = event.target.value;
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
        (error: HttpErrorResponse) => {
          this.saving = false;
          //Handle error
          this.bsModalRef.hide();
          let cc: BsModalRef;
          cc = this._modalService.show(ErrorModalComponent, {
            class: "modal-lg",
            initialState: {
              title: "",
              errorMessage: error.message,
            },
          });
        },
        () => {
          this.saving = false;
        }
      );
    }

  }

}