import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface WeightAdjustmentDto extends EntityDto<number> {
    userId: number;
    referenceNo: string;
    description: string;
    dateTime: string;
    invoiceId: number;
}

export interface PagedWeightAdjustmentResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    referenceNo?: string;
    description?: string;
    dateTime?: string;
}
