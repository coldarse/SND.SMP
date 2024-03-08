import { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface CustomerTransactionDto extends EntityDto<number> {
    wallet: string;
    customer: string;
    paymentMode: string;
    currency: string;
    transactionType: string;
    amount: number;
    referenceNo: string;
    description: string;
    transactionDate: string;
}

export interface PagedCustomerTransactionResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    wallet?: string;
    customer?: string;
    paymentMode?: string;
    currency?: string;
    transactionType?: string;
    amount?: number;
    referenceNo?: string;
    description?: string;
    transactionDate?: string;
}