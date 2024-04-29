import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface WeightAdjustmentDto extends EntityDto<number> {
    userId: number;
    referenceNo: string;
    amount: number;
    description: string;
    dateTime: string;
    weight: number;
    invoiceId: number;
}

export interface PagedWeightAdjustmentResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    userId?: number;
    referenceNo?: string;
    amount?: number;
    description?: string;
    dateTime?: string;
    weight?: number;
    invoiceId?: number;
}
