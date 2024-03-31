import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { DispatchValidationDto } from '@shared/service-proxies/dispatchvalidations/model'
import { DispatchValidationService } from '@shared/service-proxies/dispatchvalidations/dispatchvalidation.service'
import { CreateUpdateDispatchValidationComponent } from '../dispatchvalidations/create-update-dispatchvalidation/create-update-dispatchvalidation.component'

class PagedDispatchValidationsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-dispatchvalidations',
  templateUrl: './dispatchvalidations.component.html',
  styleUrls: ['./dispatchvalidations.component.css']
})
export class DispatchValidationsComponent extends PagedListingComponentBase<DispatchValidationDto> {

  keyword = '';
  dispatchvalidations: any[] = [];

  constructor(
    injector: Injector,
    private _dispatchvalidationService: DispatchValidationService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createDispatchValidation(){
    this.showCreateOrEditDispatchValidationDialog();
  }

  editDispatchValidation(entity: DispatchValidationDto){
    this.showCreateOrEditDispatchValidationDialog(entity);
  }

  private showCreateOrEditDispatchValidationDialog(entity?: DispatchValidationDto){
    let createOrEditDispatchValidationDialog: BsModalRef;
    if(!entity){
      createOrEditDispatchValidationDialog = this._modalService.show(
        CreateUpdateDispatchValidationComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditDispatchValidationDialog = this._modalService.show(
        CreateUpdateDispatchValidationComponent,
        {
          class: 'modal-lg',
          initialState: {
            dispatchvalidation: entity
          },
        }
      );
    }

    createOrEditDispatchValidationDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: DispatchValidationDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._dispatchvalidationService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.DispatchValidation.' + action);
  }

  protected list(
    request: PagedDispatchValidationsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._dispatchvalidationService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.dispatchvalidations = [];
        result.result.items.forEach((element: DispatchValidationDto) => {

          let tempDispatchValidation = {
            id: element.id,
            customerCode                    : element.customerCode                    ,
            dateStarted                     : element.dateStarted                     ,
            dateCompleted                   : element.dateCompleted                   ,
            dispatchNo                      : element.dispatchNo                      ,
            filePath                        : element.filePath                        ,
            isFundLack                      : element.isFundLack                      ,
            isValid                         : element.isValid                         ,
            postalCode                      : element.postalCode                      ,
            serviceCode                     : element.serviceCode                     ,
            productCode                     : element.productCode                     ,
            status                          : element.status                          ,
            tookInSec                       : element.tookInSec                       ,
            validationProgress              : element.validationProgress              ,
          }

          this.dispatchvalidations.push(tempDispatchValidation);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
