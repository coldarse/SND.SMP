import { Component, EventEmitter, Injector, OnInit, Output } from "@angular/core";
import { AppComponentBase } from "../../../shared/app-component-base";
import { BsModalRef } from "ngx-bootstrap/modal";

@Component({
  selector: "app-pre-post-check-weight",
  templateUrl: "./pre-post-check-weight.component.html",
  styleUrls: ["./pre-post-check-weight.component.css"],
})
export class PrePostCheckWeightComponent
  extends AppComponentBase
  implements OnInit {

    donePostCheck = false;

    @Output() isPreCheckWeight = new EventEmitter<boolean>();

    ngOnInit(): void {
        
    }

    constructor(
        injector: Injector,
        public bsModalRef: BsModalRef,
    ){
        super(injector);
    }

    onSelect(isPreCheckWeight: boolean): void {
        this.bsModalRef.hide();
        this.isPreCheckWeight.emit(isPreCheckWeight);
    }
    
  }
