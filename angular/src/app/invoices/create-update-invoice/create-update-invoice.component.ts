import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '../../../shared/app-component-base';
import { InvoiceDto } from '../../../shared/service-proxies/invoices/model';
import { InvoiceService } from '../../../shared/service-proxies/invoices/invoice.service';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-create-update-invoice',
  templateUrl: './create-update-invoice.component.html',
  styleUrls: ['./create-update-invoice.component.css']
})
export class CreateUpdateInvoiceComponent extends AppComponentBase
 implements OnInit {

  saving = false;
  isCreate = true;
  invoice?: InvoiceDto = {} as InvoiceDto;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    public _invoiceService: InvoiceService,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if(this.invoice.id != undefined){
      this.isCreate = false;
    }
  }

  save(): void {
    this.saving = true;

    if(this.invoice.id != undefined){
      this._invoiceService.update(this.invoice).subscribe(
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
      this._invoiceService.create(this.invoice).subscribe(
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
