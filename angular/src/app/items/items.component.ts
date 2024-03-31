import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { ItemDto } from '@shared/service-proxies/items/model'
import { ItemService } from '@shared/service-proxies/items/item.service'
import { CreateUpdateItemComponent } from '../items/create-update-item/create-update-item.component'

class PagedItemsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-items',
  templateUrl: './items.component.html',
  styleUrls: ['./items.component.css']
})
export class ItemsComponent extends PagedListingComponentBase<ItemDto> {

  keyword = '';
  items: any[] = [];

  constructor(
    injector: Injector,
    private _itemService: ItemService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createItem(){
    this.showCreateOrEditItemDialog();
  }

  editItem(entity: ItemDto){
    this.showCreateOrEditItemDialog(entity);
  }

  private showCreateOrEditItemDialog(entity?: ItemDto){
    let createOrEditItemDialog: BsModalRef;
    if(!entity){
      createOrEditItemDialog = this._modalService.show(
        CreateUpdateItemComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditItemDialog = this._modalService.show(
        CreateUpdateItemComponent,
        {
          class: 'modal-lg',
          initialState: {
            item: entity
          },
        }
      );
    }

    createOrEditItemDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: ItemDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._itemService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Item.' + action);
  }

  protected list(
    request: PagedItemsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._itemService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.items = [];
        result.result.items.forEach((element: ItemDto) => {

          let tempItem = {
            id: element.id,
            extID                         : element.extID                         ,
            dispatchID                    : element.dispatchID                    ,
            bagID                         : element.bagID                         ,
            dispatchDate                  : element.dispatchDate                  ,
            month                         : element.month                         ,
            postalCode                    : element.postalCode                    ,
            serviceCode                   : element.serviceCode                   ,
            productCode                   : element.productCode                   ,
            countryCode                   : element.countryCode                   ,
            weight                        : element.weight                        ,
            bagNo                         : element.bagNo                         ,
            sealNo                        : element.sealNo                        ,
            price                         : element.price                         ,
            status                        : element.status                        ,
            itemValue                     : element.itemValue                     ,
            itemDesc                      : element.itemDesc                      ,
            recpName                      : element.recpName                      ,
            telNo                         : element.telNo                         ,
            email                         : element.email                         ,
            address                       : element.address                       ,
            postcode                      : element.postcode                      ,
            rateCategory                  : element.rateCategory                  ,
            city                          : element.city                          ,
            address2                      : element.address2                      ,
            addressNo                     : element.addressNo                     ,
            state                         : element.state                         ,
            length                        : element.length                        ,
            width                         : element.width                         ,
            height                        : element.height                        ,
            hSCode                        : element.hSCode                        ,
            qty                           : element.qty                           ,
            passportNo                    : element.passportNo                    ,
            taxPayMethod                  : element.taxPayMethod                  ,
            dateStage1                    : element.dateStage1                    ,
            dateStage2                    : element.dateStage2                    ,
            dateStage3                    : element.dateStage3                    ,
            dateStage4                    : element.dateStage4                    ,
            dateStage5                    : element.dateStage5                    ,
            dateStage6                    : element.dateStage6                    ,
            dateStage7                    : element.dateStage7                    ,
            dateStage8                    : element.dateStage8                    ,
            dateStage9                    : element.dateStage9                    ,
            stage6OMTStatusDesc           : element.stage6OMTStatusDesc           ,
            stage6OMTDepartureDate        : element.stage6OMTDepartureDate        ,
            stage6OMTArrivalDate          : element.stage6OMTArrivalDate          ,
            stage6OMTDestinationCity      : element.stage6OMTDestinationCity      ,
            stage6OMTDestinationCityCode  : element.stage6OMTDestinationCityCode  ,
            stage6OMTCountryCode          : element.stage6OMTCountryCode          ,
            extMsg                        : element.extMsg                        ,
            identityType                  : element.identityType                  ,
            senderName                    : element.senderName                    ,
            iOSSTax                       : element.iOSSTax                       ,
            refNo                         : element.refNo                         ,
            dateSuccessfulDelivery        : element.dateSuccessfulDelivery        ,
            isDelivered                   : element.isDelivered                   ,
            deliveredInDays               : element.deliveredInDays               ,
            isExempted                    : element.isExempted                    ,
            exemptedRemark                : element.exemptedRemark                ,
            cLCuartel                     : element.cLCuartel                     ,
            cLSector                      : element.cLSector                      ,
            cLSDP                         : element.cLSDP                         ,
            cLCodigoDelegacionDestino     : element.cLCodigoDelegacionDestino     ,
            cLNombreDelegacionDestino     : element.cLNombreDelegacionDestino     ,
            cLDireccionDestino            : element.cLDireccionDestino            ,
            cLCodigoEncaminamiento        : element.cLCodigoEncaminamiento        ,
            cLNumeroEnvio                 : element.cLNumeroEnvio                 ,
            cLComunaDestino               : element.cLComunaDestino               ,
            cLAbreviaturaServicio         : element.cLAbreviaturaServicio         ,
            cLAbreviaturaCentro           : element.cLAbreviaturaCentro           ,
            stage1StatusDesc              : element.stage1StatusDesc              ,
            stage2StatusDesc              : element.stage2StatusDesc              ,
            stage3StatusDesc              : element.stage3StatusDesc              ,
            stage4StatusDesc              : element.stage4StatusDesc              ,
            stage5StatusDesc              : element.stage5StatusDesc              ,
            stage6StatusDesc              : element.stage6StatusDesc              ,
            stage7StatusDesc              : element.stage7StatusDesc              ,
            stage8StatusDesc              : element.stage8StatusDesc              ,
            stage9StatusDesc              : element.stage9StatusDesc              ,
            cityId                        : element.cityId                        ,
            finalOfficeId                 : element.finalOfficeId                 ,
          }

          this.items.push(tempItem);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
