import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";
import { RateItemService } from "../../../shared/service-proxies/rate-items/rate-item.service";

@Component({
  selector: "app-upload-rate-item",
  templateUrl: "./upload-rate-item.component.html",
  styleUrls: ["./upload-rate-item.component.css"],
})
export class UploadRateItemComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  formFile: any;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _rateItemService: RateItemService,
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

    this._rateItemService.uploadRateItemFile(form).subscribe(
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
