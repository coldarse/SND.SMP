import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { EmailContentDto } from '@shared/service-proxies/emailcontents/model'
import { EmailContentService } from '@shared/service-proxies/emailcontents/emailcontent.service'
import { CreateUpdateEmailContentComponent } from '../emailcontents/create-update-emailcontent/create-update-emailcontent.component'

class PagedEmailContentsRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-emailcontents',
  templateUrl: './emailcontents.component.html',
  styleUrls: ['./emailcontents.component.css']
})
export class EmailContentsComponent extends PagedListingComponentBase<EmailContentDto> {

  keyword = '';
  emailcontents: any[] = [];

  constructor(
    injector: Injector,
    private _emailcontentService: EmailContentService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createEmailContent(){
    this.showCreateOrEditEmailContentDialog();
  }

  editEmailContent(entity: EmailContentDto){
    this.showCreateOrEditEmailContentDialog(entity);
  }

  private showCreateOrEditEmailContentDialog(entity?: EmailContentDto){
    let createOrEditEmailContentDialog: BsModalRef;
    if(!entity){
      createOrEditEmailContentDialog = this._modalService.show(
        CreateUpdateEmailContentComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditEmailContentDialog = this._modalService.show(
        CreateUpdateEmailContentComponent,
        {
          class: 'modal-lg',
          initialState: {
            emailcontent: entity
          },
        }
      );
    }

    createOrEditEmailContentDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: EmailContentDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._emailcontentService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.EmailContent.' + action);
  }

  protected list(
    request: PagedEmailContentsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._emailcontentService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.emailcontents = [];
        result.result.items.forEach((element: EmailContentDto) => {

          let tempEmailContent = {
            id: element.id,
            name: element.name,
            subject: element.subject,
            content: element.content,
          }

          this.emailcontents.push(tempEmailContent);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
