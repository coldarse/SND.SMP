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
