import { Component, Injector, Input } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { DispatchDto, DispatchInfoDto } from '@shared/service-proxies/dispatches/model'
import { DispatchService } from '@shared/service-proxies/dispatches/dispatch.service'
import { CreateUpdateDispatchComponent } from '../dispatches/create-update-dispatch/create-update-dispatch.component'
import { Router } from '@angular/router';

class PagedDispatchesRequestDto extends PagedRequestDto{
  keyword: string;
  isAdmin: boolean;
  customerCode: string;
  sorting: string;
}

@Component({
  selector: 'app-dispatches',
  templateUrl: './dispatches.component.html',
  styleUrls: ['./dispatches.component.css']
})
export class DispatchesComponent extends PagedListingComponentBase<DispatchDto> {

  keyword = '';
  dispatches: any[] = [];

  @Input() showPagination: boolean = true;
  @Input() maxItems: number = 10;

  isAdmin = true;
  companyCode = "";

  constructor(
    injector: Injector,
    private router: Router,
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

  rerouteToModule() {
    this.router.navigate(["/app/dispatches"]);
  }

  downloadManifest(dispatchNo: string){
    this._dispatchService.downloadManifest(dispatchNo).subscribe(() => {
      abp.notify.success(this.l('Successfully Downloaded'));
    });
  }

  protected list(
    request: PagedDispatchesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    let admin = this.appSession
      .getShownLoginName()
      .replace(".\\", "")
      .includes("admin");
    this.isAdmin = admin;
    this.companyCode = admin ? "" : this.appSession.getCompanyCode();

    request.keyword = this.keyword;
    request.customerCode = this.companyCode;
    request.isAdmin = this.isAdmin;
    request.maxResultCount = this.maxItems;

    this._dispatchService
    .getDispatchInfoListPaged(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.dispatches = [];
        result.result.items.forEach((element: DispatchInfoDto) => {

          let tempDispatch = {
            customerName: element.customerName,
            customerCode: element.customerCode,
            postalCode: element.postalCode,
            postalDesc: element.postalDesc,
            dispatchDate: element.dispatchDate,
            dispatchNo: element.dispatchNo, 
            serviceCode: element.serviceCode,
            serviceDesc: element.serviceDesc,
            productCode: element.productCode,
            productDesc: element.productDesc,  
            totalBags: element.totalBags,   
            totalWeight: element.totalWeight,  
            totalCountry: element.totalCountry,
            status: element.status,  
          }

          this.dispatches.push(tempDispatch);
        });
        if (this.showPagination) this.showPaging(result.result, pageNumber);
    });
  }
}
