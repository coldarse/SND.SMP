import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { ApplicationSettingDto } from '@shared/service-proxies/applicationsettings/model'
import { ApplicationSettingService } from '@shared/service-proxies/applicationsettings/applicationsetting.service'
import { CreateUpdateApplicationSettingComponent } from '../applicationsettings/create-update-applicationsetting/create-update-applicationsetting.component'

class PagedApplicationSettingsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-applicationsettings',
  templateUrl: './applicationsettings.component.html',
  styleUrls: ['./applicationsettings.component.css']
})
export class ApplicationSettingsComponent extends PagedListingComponentBase<ApplicationSettingDto> {

  keyword = '';
  applicationsettings: any[] = [];

  constructor(
    injector: Injector,
    private _applicationsettingService: ApplicationSettingService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createApplicationSetting(){
    this.showCreateOrEditApplicationSettingDialog();
  }

  editApplicationSetting(entity: ApplicationSettingDto){
    this.showCreateOrEditApplicationSettingDialog(entity);
  }

  private showCreateOrEditApplicationSettingDialog(entity?: ApplicationSettingDto){
    let createOrEditApplicationSettingDialog: BsModalRef;
    if(!entity){
      createOrEditApplicationSettingDialog = this._modalService.show(
        CreateUpdateApplicationSettingComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditApplicationSettingDialog = this._modalService.show(
        CreateUpdateApplicationSettingComponent,
        {
          class: 'modal-lg',
          initialState: {
            applicationsetting: entity
          },
        }
      );
    }

    createOrEditApplicationSettingDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: ApplicationSettingDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._applicationsettingService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.ApplicationSetting.' + action);
  }

  protected list(
    request: PagedApplicationSettingsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._applicationsettingService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.applicationsettings = [];
        result.result.items.forEach((element: ApplicationSettingDto) => {

          let tempApplicationSetting = {
            id: element.id,
            name: element.name,
            value: element.value,
          }

          this.applicationsettings.push(tempApplicationSetting);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
