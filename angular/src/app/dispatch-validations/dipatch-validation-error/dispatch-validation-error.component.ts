import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { DispatchValidateDto } from "@shared/service-proxies/dispatch-validations/model";

@Component({
  selector: "app-dispatch-validation-error",
  templateUrl: "./dispatch-validation-error.component.html",
  styleUrls: ["./dispatch-validation-error.component.css"],
})
export class DispatchValidationErrorComponent implements OnInit {
  errors: DispatchValidateDto[] = [];
  dispatchNo: string = "";

  @Output() onClose = new EventEmitter<any>();

  constructor(private _chibiService: ChibiService) {}

  ngOnInit(): void {
    this._chibiService
      .getDispatchValidationError(this.dispatchNo)
      .subscribe((result: any) => {
        this.errors = result.result;
      });
  }

  closeDialog() {
    this.onClose.emit();
  }
}
