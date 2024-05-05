import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { IMPCDto } from '@shared/service-proxies/impcs/model'
import { IMPCService } from '@shared/service-proxies/impcs/impc.service'
import { CreateUpdateIMPCComponent } from '../impcs/create-update-impc/create-update-impc.component'

class PagedIMPCSRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-impcs',
  templateUrl: './impcs.component.html',
  styleUrls: ['./impcs.component.css']
})
export class IMPCSComponent extends PagedListingComponentBase<IMPCDto> {

  keyword = '';
  impcs: any[] = [];

  constructor(
    injector: Injector,
    private _impcService: IMPCService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createIMPC(){
    this.showCreateOrEditIMPCDialog();
  }

  editIMPC(entity: IMPCDto){
    this.showCreateOrEditIMPCDialog(entity);
  }

  private showCreateOrEditIMPCDialog(entity?: IMPCDto){
    let createOrEditIMPCDialog: BsModalRef;
    if(!entity){
      createOrEditIMPCDialog = this._modalService.show(
        CreateUpdateIMPCComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditIMPCDialog = this._modalService.show(
        CreateUpdateIMPCComponent,
        {
          class: 'modal-lg',
          initialState: {
            impc: entity
          },
        }
      );
    }

    createOrEditIMPCDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: IMPCDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._impcService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.IMPC.' + action);
  }

  protected list(
    request: PagedIMPCSRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._impcService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.impcs = [];
        result.result.items.forEach((element: IMPCDto) => {

          let tempIMPC = {
            id: element.id,
            type: element.type,
            countryCode: element.countryCode,
            airportCode: element.airportCode,
            iMPCCode: element.iMPCCode,
            logisticCode: element.logisticCode,
          }

          this.impcs.push(tempIMPC);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
