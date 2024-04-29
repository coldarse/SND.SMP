import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RefundDto extends EntityDto<number> {
    userId: number;
    referenceNo: string;
    amount: number;
    description: string;
    dateTime: string;
    weight: number;
}

export interface PagedRefundResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    userId?: number;
    referenceNo?: string;
    amount?: number;
    description?: string;
    dateTime?: string;
    weight?: number;
}
