import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { AirportDto } from '@shared/service-proxies/airports/model'
import { AirportService } from '@shared/service-proxies/airports/airport.service'
import { CreateUpdateAirportComponent } from '../airports/create-update-airport/create-update-airport.component'
import { PostalCountryService } from '@shared/service-proxies/postal-countries/postal-country.service';

class PagedAirportsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-airports',
  templateUrl: './airports.component.html',
  styleUrls: ['./airports.component.css']
})
export class AirportsComponent extends PagedListingComponentBase<AirportDto> {

  keyword = '';
  airports: any[] = [];
  countries: string[] = [];

  constructor(
    injector: Injector,
    private _airportService: AirportService,
    private _postalCountryService: PostalCountryService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createAirport(){
    this.showCreateOrEditAirportDialog();
  }

  editAirport(entity: AirportDto){
    this.showCreateOrEditAirportDialog(entity);
  }

  private showCreateOrEditAirportDialog(entity?: AirportDto){
    let createOrEditAirportDialog: BsModalRef;
    if(!entity){
      createOrEditAirportDialog = this._modalService.show(
        CreateUpdateAirportComponent,
        {
          class: 'modal-lg',
          initialState: {
            countries: this.countries
          },
        }
      );
    }
    else{
      createOrEditAirportDialog = this._modalService.show(
        CreateUpdateAirportComponent,
        {
          class: 'modal-lg',
          initialState: {
            airport: entity,
            countries: this.countries
          },
        }
      );
    }

    createOrEditAirportDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: AirportDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._airportService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Airport.' + action);
  }

  protected list(
    request: PagedAirportsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._airportService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.airports = [];
      result.result.items.forEach((element: AirportDto) => {

        let tempAirport = {
          id: element.id,
          name: element.name,
          code: element.code,
          country: element.country,
        }

        this.airports.push(tempAirport);
      });
      this._postalCountryService.getCountries().subscribe((data: any) => {
        this.countries = data.result;
        this.showPaging(result.result, pageNumber);
      });
    });
  }
}
