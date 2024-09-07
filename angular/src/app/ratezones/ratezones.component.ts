import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { RateZoneDto } from '@shared/service-proxies/ratezones/model'
import { RateZoneService } from '@shared/service-proxies/ratezones/ratezone.service'
import { CreateUpdateRateZoneComponent } from '../ratezones/create-update-ratezone/create-update-ratezone.component'

class PagedRateZonesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-ratezones',
  templateUrl: './ratezones.component.html',
  styleUrls: ['./ratezones.component.css']
})
export class RateZonesComponent extends PagedListingComponentBase<RateZoneDto> {

  keyword = '';
  ratezones: any[] = [];

  constructor(
    injector: Injector,
    private _ratezoneService: RateZoneService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createRateZone(){
    this.showCreateOrEditRateZoneDialog();
  }

  editRateZone(entity: RateZoneDto){
    this.showCreateOrEditRateZoneDialog(entity);
  }

  private showCreateOrEditRateZoneDialog(entity?: RateZoneDto){
    let createOrEditRateZoneDialog: BsModalRef;
    if(!entity){
      createOrEditRateZoneDialog = this._modalService.show(
        CreateUpdateRateZoneComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditRateZoneDialog = this._modalService.show(
        CreateUpdateRateZoneComponent,
        {
          class: 'modal-lg',
          initialState: {
            ratezone: entity
          },
        }
      );
    }

    createOrEditRateZoneDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: RateZoneDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._ratezoneService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.RateZone.' + action);
  }

  protected list(
    request: PagedRateZonesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._ratezoneService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.ratezones = [];
        result.result.items.forEach((element: RateZoneDto) => {

          let tempRateZone = {
            id: element.id,
            zone: element.zone,
            state: element.state,
            city: element.city,
            postCode: element.postCode,
          }

          this.ratezones.push(tempRateZone);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
