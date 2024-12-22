import { Component, Injector, OnInit } from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";

@Component({
  selector: "app-view-body",
  templateUrl: "./view-body.component.html",
  styleUrls: ["./view-body.component.css"],
})
export class ViewBodyComponent extends AppComponentBase implements OnInit {

  isRequest: boolean;
  body: string;
  datetime: Date;

  bodyJSON: any;

  constructor(
    injector: Injector,
    public bsModalRef: BsModalRef,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.bodyJSON = JSON.parse(this.body);
  }
}
