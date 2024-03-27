import { Component, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { QueueDto } from '@shared/service-proxies/queues/model'
import { QueueService } from '@shared/service-proxies/queues/queue.service'
import { CreateUpdateQueueComponent } from '../queues/create-update-queue/create-update-queue.component'

class PagedQueuesRequestDto extends PagedRequestDto{
  keyword: string
}

@Component({
  selector: 'app-queues',
  templateUrl: './queues.component.html',
  styleUrls: ['./queues.component.css']
})
export class QueuesComponent extends PagedListingComponentBase<QueueDto> {

  keyword = '';
  queues: any[] = [];

  constructor(
    injector: Injector,
    private _queueService: QueueService,
    private _modalService: BsModalService
  ){
    super(injector);
  }

  createQueue(){
    this.showCreateOrEditQueueDialog();
  }

  editQueue(entity: QueueDto){
    this.showCreateOrEditQueueDialog(entity);
  }

  private showCreateOrEditQueueDialog(entity?: QueueDto){
    let createOrEditQueueDialog: BsModalRef;
    if(!entity){
      createOrEditQueueDialog = this._modalService.show(
        CreateUpdateQueueComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    else{
      createOrEditQueueDialog = this._modalService.show(
        CreateUpdateQueueComponent,
        {
          class: 'modal-lg',
          initialState: {
            queue: entity
          },
        }
      );
    }

    createOrEditQueueDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  clearFilters(): void {
    this.keyword = '';
    this.getDataPage(1);
  }

  protected delete(entity: QueueDto): void{
    abp.message.confirm(
      '',
      undefined,
      (result: boolean) => {
        if (result) {
          this._queueService.delete(entity.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted('Pages.Queue.' + action);
  }

  protected list(
    request: PagedQueuesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._queueService
    .getAll(
      request
    ).pipe(
      finalize(() => {
        finishedCallback();
      })
    )
    .subscribe((result: any) => {
      this.queues = [];
        result.result.items.forEach((element: QueueDto) => {

          let tempQueue = {
            id: element.id,
            eventType: element.eventType,
            filePath: element.filePath,
            deleteFileOnSuccess: element.deleteFileOnSuccess,
            deleteFileOnFailed: element.deleteFileOnFailed,
            dateCreated: element.dateCreated,
            status: element.status,
            tookInSec: element.tookInSec,
            errorMsg: element.errorMsg,
            startTime: element.startTime,
            endTime: element.endTime,
          }

          this.queues.push(tempQueue);
        });
      this.showPaging(result.result, pageNumber);
    });
  }
}
