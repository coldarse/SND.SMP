import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface InvoiceDto extends EntityDto<number> {
    dateTime: string;
    invoiceNo: string;
    customer: string;
}

export interface PagedInvoiceResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    dateTime?: string;
    invoiceNo?: string;
    customer?: string;
}

export interface GenerateInvoice {
    customer: string;
    invoiceNo: string;
    invoiceDate: string;
    dispatches: string[];
    billTo: string;
    extraCharges: ExtraCharge[];
}

export interface ExtraCharge {
    description: string;
    weight: number;
    country: string;
    ratePerKG: number;
    quantity: number;
    unitPrice: number;
    amount: number;
    currency: string;
}
