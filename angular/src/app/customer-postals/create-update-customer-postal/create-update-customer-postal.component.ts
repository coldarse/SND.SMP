import {
  Component,
  EventEmitter,
  Injector,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { CustomerPostalDto, DetailedCustomerPostalDto } from "../../../shared/service-proxies/customer-postals/model";
import { CustomerPostalService } from "../../../shared/service-proxies/customer-postals/customer-postal.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { RateDDL } from "@shared/service-proxies/rates/model";
import { PostalDDL } from "@shared/service-proxies/postals/model";
import { CustomerDto } from "@shared/service-proxies/customers/model";
import { ErrorModalComponent } from "../../../shared/components/error-modal/error-modal.component";
import { HttpErrorResponse } from "@angular/common/http";
import { WalletService } from "@shared/service-proxies/wallets/wallet.service";
import { WalletDto } from "@shared/service-proxies/wallets/model";

@Component({
  selector: "app-create-update-customerpostal",
  templateUrl: "./create-update-customer-postal.component.html",
  styleUrls: ["./create-update-customer-postal.component.css"],
})
export class CreateUpdateCustomerPostalComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  isCreate = true;
  customerpostal?: DetailedCustomerPostalDto = {} as DetailedCustomerPostalDto;

  postalItems: PostalDDL[];

  rateItems: RateDDL[];

  formInvalid = true;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _customerpostalService: CustomerPostalService,
    public _walletService: WalletService,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    // if(this.customerpostal.id != undefined){
    //   this.isCreate = false;
    // }
    this.isCreate = true;
    // console.log(this.postalItems)
  }

  selectedPostal(event: any) {
    this.customerpostal.postal = event.target.value;
    if (
      this.customerpostal.rate == undefined ||
      this.customerpostal.rate == 0 ||
      this.customerpostal.postal == ""
    )
      this.formInvalid = true;
    else this.formInvalid = false;
  }

  selectedRate(event: any) {
    this.customerpostal.rate = +event.target.value;
    if (
      this.customerpostal.postal == undefined ||
      this.customerpostal.postal == "" ||
      this.customerpostal.rate == 0
    )
      this.formInvalid = true;
    else this.formInvalid = false;
  }

  save(): void {
    this.saving = true;

    if (this.customerpostal.id != undefined) {
      this._customerpostalService.update(this.customerpostal).subscribe(
        () => {
          this.notify.info(this.l("SavedSuccessfully"));
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
    } else {
      this._customerpostalService
        .isCurrencyWalletExist(
          this.customerpostal.rate,
          this.customerpostal.accountNo
        )
        .subscribe((data: any) => {
          if (data.result.exists) {
            this._customerpostalService.create(this.customerpostal).subscribe(
              () => {
                this.notify.info(this.l("SavedSuccessfully"));
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
          } else {
            this.bsModalRef.hide();
            let cc: BsModalRef;
            cc = this._modalService.show(ErrorModalComponent, {
              class: "modal-lg",
              initialState: {
                title: "",
                yesno: true,
                errorMessage: `This Customer does not have a wallet for ${data.result.currencyDesc} currency. Do you want to help them create this wallet and proceed with Customer Postal creation?`,
              },
            });

            cc.content.yesClick.subscribe(() => {
              this.customerpostal.createWallet = data.result;
              this.customerpostal.createWallet.create = true;
              this._customerpostalService.create(this.customerpostal).subscribe(
                () => {
                  this.notify.info(this.l("SavedSuccessfully"));
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
            });
          }
        });
    }
  }
}
