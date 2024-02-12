import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { EWalletTypeDto } from '@shared/service-proxies/ewallettypes/model'
import { EWalletTypeService } from '@shared/service-proxies/ewallettypes/ewallettype.service'
import { CreateUpdateEWalletTypeComponent } from '../ewallettypes/create-update-ewallettype/create-update-ewallettype.component'

class PagedEWalletTypesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-ewallettypes',
  templateUrl: './ewallettypes.component.html',
  styleUrls: ['./ewallettypes.component.css']
})
export class EWalletTypesComponent extends PagedListingComponentBase<EWalletTypeDto> {

  keyword = '';
  ewallettypes: any[] = [];

  constructor(
    injector: Injector,
    private _ewallettypeService: EWalletTypeService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createEWalletType(){
    this.showCreateOrEditEWalletTypeDialog();
  }

  editEWalletType(entity: EWalletTypeDto){
    this.showCreateOrEditEWalletTypeDialog(entity);
  }

  private showCreateOrEditEWalletTypeDialog(entity?: EWalletTypeDto){
    let createOrEditEWalletTypeDialog: BsModalRef;
    if(!entity){
      createOrEditEWalletTypeDialog = this._modalService.show(
        CreateUpdateEWalletTypeComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditEWalletTypeDialog = this._modalService.show(
        CreateUpdateEWalletTypeComponent,
        {
          class: 'modal-lg',
          initialState: {
            ewallettype: entity
          },
        }
      );
    }

    createOrEditEWalletTypeDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: EWalletTypeDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._ewallettypeService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.EWalletType.' + action);
  }

  protected list(
    request: PagedEWalletTypesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._ewallettypeService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.ewallettypes = [];
        result.result.items.forEach((element: EWalletTypeDto) => {

          let tempEWalletType = {
            id: element.id,
            type: element.type,
          }

          this.ewallettypes.push(tempEWalletType);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
