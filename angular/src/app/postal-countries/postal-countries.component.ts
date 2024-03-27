import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { PostalCountryDto } from '@shared/service-proxies/postal-countries/model';
import { PostalCountryService } from '@shared/service-proxies/postal-countries/postal-country.service';
import { UploadPostalCountryComponent } from '../postal-countries/upload-postal-country/upload-postal-country.component';

class PagedPostalCountriesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-postal-countries',
  templateUrl: './postal-countries.component.html',
  styleUrls: ['./postal-countries.component.css']
})
export class PostalCountriesComponent extends PagedListingComponentBase<PostalCountryDto> {

  keyword = '';
  postalcountries: any[] = [];

  constructor(
    injector: Injector,
    private _postalcountryService: PostalCountryService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  uploadPostalCountry() {
    this.showUploadPostalCountryDialog();
  }

  private showUploadPostalCountryDialog() {
    let uploadPostalDialog: BsModalRef;
    uploadPostalDialog = this._modalService.show(UploadPostalCountryComponent, {
      class: "modal-lg",
    });

    uploadPostalDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: PostalCountryDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._postalcountryService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.PostalCountry.' + action);
  }

  protected list(
    request: PagedPostalCountriesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._postalcountryService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.postalcountries = [];
        result.result.items.forEach((element: PostalCountryDto) => {

          let tempPostalCountry = {
            id: element.id,
            postalCode: element.postalCode,
            countryCode: element.countryCode,
          }

          this.postalcountries.push(tempPostalCountry);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
