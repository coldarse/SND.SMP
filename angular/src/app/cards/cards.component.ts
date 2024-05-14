import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerTransactionService } from '@shared/service-proxies/customer-transactions/customer-transaction.service';
import { CustomerTransactionDto } from '@shared/service-proxies/customer-transactions/model';
import { DispatchValidationService } from '@shared/service-proxies/dispatch-validations/dispatch-validation.service';
import { DispatchService } from '@shared/service-proxies/dispatches/dispatch.service';
import { DispatchValidationDto } from '@shared/service-proxies/dispatch-validations/model';
import { DispatchInfoDto } from '@shared/service-proxies/dispatches/model';

@Component({
  selector: 'app-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cards.component.html',
  styleUrl: './cards.component.css'
})
export class CardsComponent implements OnInit{

  @Input() isAdmin: boolean; 
  @Input() Customer: string;

  selected: string = '1';
  @Output() selected_dashboard_item = new EventEmitter<string>();

  transactions: CustomerTransactionDto[] = [];
  validations: DispatchValidationDto[] =[];
  dispatches: DispatchInfoDto[] =[];

  constructor(
    private _customerTransactionService: CustomerTransactionService,
    private _dispatchValidationService: DispatchValidationService,
    private _dispatchService: DispatchService,
  ){}

  selectItem(item: string) {
    this.selected = item;
    this.selected_dashboard_item.emit(this.selected);
  }

  ngOnInit(): void {
    this._customerTransactionService.getDashboardTransaction(this.isAdmin, 3, this.Customer).subscribe((data: any) => {
      this.transactions = data.result;
    });

    this._dispatchValidationService.getDashboardDispatchValidation(this.isAdmin, 3, this.Customer).subscribe((data: any) => {
      this.validations = data.result;
    });

    this._dispatchService.getDashboardDispatchInfo(this.isAdmin, 3, this.Customer).subscribe((data: any) => {
      this.dispatches = data.result;
    });
  }
}
