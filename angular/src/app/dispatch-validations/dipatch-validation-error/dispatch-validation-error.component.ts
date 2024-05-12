import { HttpErrorResponse } from "@angular/common/http";
import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { ErrorModalComponent } from "@shared/components/error-modal/error-modal.component";
import { ChibiService } from "@shared/service-proxies/chibis/chibis.service";
import { DispatchValidateDto } from "@shared/service-proxies/dispatch-validations/model";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";

@Component({
  selector: "app-dispatch-validation-error",
  templateUrl: "./dispatch-validation-error.component.html",
  styleUrls: ["./dispatch-validation-error.component.css"],
})
export class DispatchValidationErrorComponent implements OnInit {
  errors: DispatchValidateDto[] = [];
  dispatchNo: string = "";

  @Output() onClose = new EventEmitter<any>();

  constructor(
    private _chibiService: ChibiService,
    private _modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this._chibiService
      .getDispatchValidationError(this.dispatchNo)
      .subscribe((result: any) => {
        this.errors = result.result;
      }
    ,(error: HttpErrorResponse) => {
      let cc: BsModalRef;
        cc = this._modalService.show(ErrorModalComponent, {
          class: "modal-lg",
          initialState: {
            title: "",
            errorMessage: error.message,
          },
        });
    });
  }

  closeDialog() {
    this.onClose.emit();
  }
}
