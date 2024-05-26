import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ItemTrackingReviewDto extends EntityDto<number> {
    applicationId: number;
    customerId: number;
    customerCode: string;
    postalCode: string;
    postalDesc: string;
    total: number;
    totalGiven: number;
    productCode: string;
    status: string;
    dateCreated: string;
    prefix: string;
    prefixNo: string;
    suffix: string;
    remark: string;
}

export interface PagedItemTrackingReviewResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    customerCode?: string;
    postalCode?: string;
    postalDesc?: string;
    productCode?: string;
    status?: string;
    dateCreated?: string;
    prefix?: string;
    prefixNo?: string;
    suffix?: string;
    remark?: string;
}

export interface ReviewAmount {
    issued: string;
    remaining: string;
    uploaded: string;
}
