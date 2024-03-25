import {
  Component,
  EventEmitter,
  Injector,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";

import { RateWeightBreakService } from "@shared/service-proxies/rateweightbreaks/rateweightbreak.service";

@Component({
  selector: "app-upload-rate-weight-break",
  templateUrl: "./upload-rate-weight-break.component.html",
  styleUrls: ["./upload-rate-weight-break.component.css"],
})
export class UploadRateWeightBreakComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  formFile: any;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _rateItemService: RateWeightBreakService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {}

  handleUpload(event) {
    if (event.target.files.length > 0) {
      this.formFile = event.target.files[0];
    }
  }

  save(): void {
    this.saving = true;

    const form = new FormData();
    form.append("file", this.formFile);

    this._rateItemService.uploadRateWeightBreakFile(form).subscribe(
      () => {
        this.notify.info(this.l("UploadedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      },
      () => {
        this.saving = false;
      }
    );
  }
}
