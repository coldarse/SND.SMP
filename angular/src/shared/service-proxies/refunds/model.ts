import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RefundDto extends EntityDto<number> {
    userId: number;
    referenceNo: string;
    description: string;
    dateTime: string;
}

export interface PagedRefundResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    referenceNo?: string;
    description?: string;
    dateTime?: string;
}
