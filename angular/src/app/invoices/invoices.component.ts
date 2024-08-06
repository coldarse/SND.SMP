import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { InvoiceDto } from '@shared/service-proxies/invoices/model'
import { InvoiceService } from '@shared/service-proxies/invoices/invoice.service'
import { CreateUpdateInvoiceComponent } from '../invoices/create-update-invoice/create-update-invoice.component'

class PagedInvoicesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css']
})
export class InvoicesComponent extends PagedListingComponentBase<InvoiceDto> {

  keyword = '';
  invoices: any[] = [];

  constructor(
    injector: Injector,
    private _invoiceService: InvoiceService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createInvoice(){
    this.showCreateOrEditInvoiceDialog();
  }

  editInvoice(entity: InvoiceDto){
    this.showCreateOrEditInvoiceDialog(entity);
  }

  private showCreateOrEditInvoiceDialog(entity?: InvoiceDto){
    let createOrEditInvoiceDialog: BsModalRef;
    if(!entity){
      createOrEditInvoiceDialog = this._modalService.show(
        CreateUpdateInvoiceComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditInvoiceDialog = this._modalService.show(
        CreateUpdateInvoiceComponent,
        {
          class: 'modal-lg',
          initialState: {
            invoice: entity
          },
        }
      );
    }

    createOrEditInvoiceDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: InvoiceDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._invoiceService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Invoice.' + action);
  }

  protected list(
    request: PagedInvoicesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._invoiceService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.invoices = [];
        result.result.items.forEach((element: InvoiceDto) => {

          let tempInvoice = {
            id: element.id,
            dateTime: element.dateTime,
            invoiceNo: element.invoiceNo,
            customer: element.customer,
          }

          this.invoices.push(tempInvoice);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
