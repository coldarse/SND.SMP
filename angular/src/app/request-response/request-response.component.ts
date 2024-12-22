import { Component, Injector } from "@angular/core";
import { duration } from "@node_modules/moment-timezone";
import {
  PagedListingComponentBase,
  PagedRequestDto,
  PagedResultDto,
} from "@shared/paged-listing-component-base";
import { RequestResponseDto } from "@shared/service-proxies/request-responses/model";
import { RequestResponseService } from "@shared/service-proxies/request-responses/request-responses.service";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { ViewBodyComponent } from "./view-body/view-body.component";

class PagedRequestResponsesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  selector: "app-request-response",
  templateUrl: "./request-response.component.html",
  styleUrls: ["./request-response.component.css"],
})
export class RequestResponseComponent extends PagedListingComponentBase<RequestResponseDto> {
  keyword = "";
  requestResponses: any[] = [];

  constructor(
    injector: Injector,
    private _requestResponseService: RequestResponseService,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  showViewBodyDialog(entity: RequestResponseDto, isRequest: boolean) {
    let viewBodyDialog: BsModalRef;
    let body = isRequest ? entity.requestBody : entity.responseBody;
    let datetime = isRequest ? entity.requestDateTime : entity.responseDateTime;

    viewBodyDialog = this._modalService.show(
      ViewBodyComponent,
      {
        class: "modal-xl",
        backdrop: true,
        initialState: {
          isRequest:isRequest, 
          body: body,
          datetime: datetime
        },
      }
    );
  }

  clearFilters(): void {
    this.keyword = "";
    this.getDataPage(1);
  }

  protected delete(entity: RequestResponseDto): void {
    abp.message.confirm("", undefined, (result: boolean) => {
      if (result) {
        this._requestResponseService.delete(entity.id).subscribe(() => {
          abp.notify.success(this.l("SuccessfullyDeleted"));
          this.refresh();
        });
      }
    });
  }

  isButtonVisible(action: string): boolean {
    return this.permission.isGranted("Pages.RequestResponse." + action);
  }

  protected list(
    request: PagedRequestResponsesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;
    this._requestResponseService
      .getAll(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.requestResponses = [];
        result.result.items.forEach((element: RequestResponseDto) => {
          let tempRequestResponse = {
            id: element.id,
            url: element.url,
            requestBody: element.requestBody,
            responseBody: element.responseBody,
            date: element.requestDateTime,
            requestDateTime: element.requestDateTime,
            responseDateTime: element.responseDateTime,
            duration: element.duration,
          };

          this.requestResponses.push(tempRequestResponse);
        });
        this.showPaging(result.result, pageNumber);
      });
  }
}
