import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { PostalDto } from '@shared/service-proxies/postals/model'
import { PostalService } from '@shared/service-proxies/postals/postal.service'
import { UploadPostalComponent } from './upload-postal/upload-postal.component';
import { CreateUpdatePostalComponent } from './create-update-postal/create-update-postal.component';
import * as XLSX from "xlsx";
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorModalComponent } from '@shared/components/error-modal/error-modal.component';

class PagedPostalsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-postals',
  templateUrl: './postals.component.html',
  styleUrls: ['./postals.component.css']
})
export class PostalsComponent extends PagedListingComponentBase<PostalDto> {

  keyword = '';
  postals: any[] = [];

  constructor(
    injector: Injector,
    private _postalService: PostalService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  uploadPostal(){
    this.showUploadPostalDialog();
  }

  private showUploadPostalDialog() {
    let uploadPostalDialog: BsModalRef;
    uploadPostalDialog = this._modalService.show(UploadPostalComponent, {
      class: "modal-lg",
    });

    uploadPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  createPostal(){
    this.showCreateOrEditPostalDialog();
  }

  editPostal(entity: PostalDto){
    this.showCreateOrEditPostalDialog(entity);
  }

  private showCreateOrEditPostalDialog(entity?: PostalDto){
    let createOrEditPostalDialog: BsModalRef;
    if(!entity){
      createOrEditPostalDialog = this._modalService.show(
        CreateUpdatePostalComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditPostalDialog = this._modalService.show(
        CreateUpdatePostalComponent,
        {
          class: 'modal-lg',
          initialState: {
            postal: entity
          },
        }
      );
    }

    createOrEditPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: PostalDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._postalService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          },
          (error: HttpErrorResponse) => {
            //Handle error
            let cc: BsModalRef;
            cc = this._modalService.show(
              ErrorModalComponent,
              {
                class: 'modal-lg',
                initialState: {
                  title: "",
                  errorMessage: error.message,
                },
              }
            )
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Postal.' + action);
  }

  protected list(
    request: PagedPostalsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._postalService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.postals = [];
        result.result.items.forEach((element: PostalDto) => {

          let tempPostal = {
            id: element.id,
            postalCode: element.postalCode,
            postalDesc: element.postalDesc,
            serviceCode: element.serviceCode,
            serviceDesc: element.serviceDesc,
            productCode: element.productCode,
            productDesc: element.productDesc,
            itemTopUpValue: element.itemTopUpValue,
          }

          this.postals.push(tempPostal);
        });
      this.showPaging(result.result, pageNumber);
    });
  }

  downloadTemplate(){
    let postalHeaders: string[] = [
      "Postal Code",
      "Postal Description",
      "Service Description",
      "Service Code",
      "Product Description",
      "Product Code",
      "Item Top-Up Value",
    ];
    
    let postalTemplate: string[][] = [postalHeaders, []];

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(postalTemplate);
    ws['!cols'] = this.fitToColumn(postalHeaders);
    XLSX.utils.book_append_sheet(wb, ws, "Sheet 1");
    XLSX.writeFile(wb, `PostalUpload.xlsx`);
  }

  fitToColumn(arrayOfArray: any[]) {
    let widths = [];
    arrayOfArray.forEach((elem) => {
      widths.push({
        wch: Math.max(elem.length) + 1
      });
    })

    return widths;
  }
}
