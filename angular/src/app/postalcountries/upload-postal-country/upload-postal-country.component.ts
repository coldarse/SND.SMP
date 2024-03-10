import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";
import { PostalCountryService } from "../../../shared/service-proxies/postalcountries/postalcountry.service";

@Component({
  selector: "app-upload-postal-country",
  templateUrl: "./upload-postal-country.component.html",
  styleUrls: ["./upload-postal-country.component.css"],
})
export class UploadPostalCountryComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  formFile: any;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _postalCountryService: PostalCountryService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {}

  handleUpload(event){
    if(event.target.files.length > 0){
        this.formFile = event.target.files[0];
    }
  }

  save(): void {
    this.saving = true;

    const form = new FormData();
    form.append('file', this.formFile);

    this._postalCountryService.uploadPostalCountryFile(form).subscribe(
      () => {
        this.notify.info(this.l("UploadedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      },
      () => { this.saving = false;
      }
    );
  }
}
