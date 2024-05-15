import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";

@Component({
  selector: "app-upload-retry",
  templateUrl: "./upload-retry.component.html",
  styleUrls: ["./upload-retry.component.css"],
})
export class UploadRetryComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  formFile: any = undefined;

  filepath: string;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _chibiService: ChibiService,
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
    if (this.formFile != undefined) 
      form.append("UploadFile.file", this.formFile);
    form.append("path", this.filepath);

    this._chibiService.uploadRetryPreCheckFile(form).subscribe(
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
