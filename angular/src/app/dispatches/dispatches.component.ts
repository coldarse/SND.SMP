import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { DispatchDto } from '@shared/service-proxies/dispatches/model'
import { DispatchService } from '@shared/service-proxies/dispatches/dispatch.service'
import { CreateUpdateDispatchComponent } from '../dispatches/create-update-dispatch/create-update-dispatch.component'

class PagedDispatchesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-dispatches',
  templateUrl: './dispatches.component.html',
  styleUrls: ['./dispatches.component.css']
})
export class DispatchesComponent extends PagedListingComponentBase<DispatchDto> {

  keyword = '';
  dispatches: any[] = [];

  constructor(
    injector: Injector,
    private _dispatchService: DispatchService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createDispatch(){
    this.showCreateOrEditDispatchDialog();
  }

  editDispatch(entity: DispatchDto){
    this.showCreateOrEditDispatchDialog(entity);
  }

  private showCreateOrEditDispatchDialog(entity?: DispatchDto){
    let createOrEditDispatchDialog: BsModalRef;
    if(!entity){
      createOrEditDispatchDialog = this._modalService.show(
        CreateUpdateDispatchComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditDispatchDialog = this._modalService.show(
        CreateUpdateDispatchComponent,
        {
          class: 'modal-lg',
          initialState: {
            dispatch: entity
          },
        }
      );
    }

    createOrEditDispatchDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: DispatchDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._dispatchService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Dispatch.' + action);
  }

  protected list(
    request: PagedDispatchesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._dispatchService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.dispatches = [];
        result.result.items.forEach((element: DispatchDto) => {

          let tempDispatch = {
            id: element.id,
            customerCode                    : element.customerCode                    ,
            pOBox                           : element.pOBox                           ,
            pPI                             : element.pPI                             ,
            postalCode                      : element.postalCode                      ,
            serviceCode                     : element.serviceCode                     ,
            productCode                     : element.productCode                     ,
            dispatchDate                    : element.dispatchDate                    ,
            dispatchNo                      : element.dispatchNo                      ,
            eTAtoHKG                        : element.eTAtoHKG                        ,
            flightTrucking                  : element.flightTrucking                  ,
            batchId                         : element.batchId                         ,
            isPayment                       : element.isPayment                       ,
            noofBag                         : element.noofBag                         ,
            itemCount                       : element.itemCount                       ,
            totalWeight                     : element.totalWeight                     ,
            totalPrice                      : element.totalPrice                      ,
            status                          : element.status                          ,
            isActive                        : element.isActive                        ,
            cN38                            : element.cN38                            ,
            transactionDateTime             : element.transactionDateTime             ,
            aTA                             : element.aTA                             ,
            postCheckTotalBags              : element.postCheckTotalBags              ,
            postCheckTotalWeight            : element.postCheckTotalWeight            ,
            airportHandling                 : element.airportHandling                 ,
            remark                          : element.remark                          ,
            weightGap                       : element.weightGap                       ,
            weightAveraged                  : element.weightAveraged                  ,
            dateSOAProcessCompleted         : element.dateSOAProcessCompleted         ,
            sOAProcessCompletedByID         : element.sOAProcessCompletedByID         ,
            totalWeightSOA                  : element.totalWeightSOA                  ,
            totalAmountSOA                  : element.totalAmountSOA                  ,
            performanceDaysDiff             : element.performanceDaysDiff             ,
            datePerformanceDaysDiff         : element.datePerformanceDaysDiff         ,
            airlineCode                     : element.airlineCode                     ,
            flightNo                        : element.flightNo                        ,
            portDeparture                   : element.portDeparture                   ,
            extDispatchNo                   : element.extDispatchNo                   ,
            dateFlight                      : element.dateFlight                      ,
            airportTranshipment             : element.airportTranshipment             ,
            officeDestination               : element.officeDestination               ,
            officeOrigin                    : element.officeOrigin                    ,
            stage1StatusDesc                : element.stage1StatusDesc                ,
            stage2StatusDesc                : element.stage2StatusDesc                ,
            stage3StatusDesc                : element.stage3StatusDesc                ,
            stage4StatusDesc                : element.stage4StatusDesc                ,
            stage5StatusDesc                : element.stage5StatusDesc                ,
            stage6StatusDesc                : element.stage6StatusDesc                ,
            stage7StatusDesc                : element.stage7StatusDesc                ,
            stage8StatusDesc                : element.stage8StatusDesc                ,
            stage9StatusDesc                : element.stage9StatusDesc                ,
            dateStartedAPI                  : element.dateStartedAPI                  ,
            dateEndedAPI                    : element.dateEndedAPI                    ,
            statusAPI                       : element.statusAPI                       ,
            countryOfLoading                : element.countryOfLoading                ,
            dateFlightArrival               : element.dateFlightArrival               ,
            postManifestSuccess             : element.postManifestSuccess             ,
            postManifestMsg                 : element.postManifestMsg                 ,
            postManifestDate                : element.postManifestDate                ,
            postDeclarationSuccess          : element.postDeclarationSuccess          ,
            postDeclarationMsg              : element.postDeclarationMsg              ,
            postDeclarationDate             : element.postDeclarationDate             ,
            airwayBLNo                      : element.airwayBLNo                      ,
            airwayBLDate                    : element.airwayBLDate                    ,
            dateLocalDelivery               : element.dateLocalDelivery               ,
            dateCLStage1Submitted           : element.dateCLStage1Submitted           ,
            dateCLStage2Submitted           : element.dateCLStage2Submitted           ,
            dateCLStage3Submitted           : element.dateCLStage3Submitted           ,
            dateCLStage4Submitted           : element.dateCLStage4Submitted           ,
            dateCLStage5Submitted           : element.dateCLStage5Submitted           ,
            dateCLStage6Submitted           : element.dateCLStage6Submitted           ,
            bRCN38RequestId                 : element.bRCN38RequestId                 ,
            dateArrival                     : element.dateArrival                     ,
            dateAcceptanceScanning          : element.dateAcceptanceScanning          ,
            seqNo                           : element.seqNo                           ,
            cORateOptionId                  : element.cORateOptionId                  ,
            paymentMode                     : element.paymentMode                     ,
            currencyId                      : element.currencyId                      ,
            importProgress                  : element.importProgress                  ,
          }

          this.dispatches.push(tempDispatch);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
