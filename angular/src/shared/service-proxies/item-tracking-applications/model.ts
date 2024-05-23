import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ItemTrackingApplicationDto extends EntityDto<number> {
    customerId: number;
    customerCode: string;
    postalCode: string;
    postalDesc: string;
    total: number;
    productCode: string;
    productDesc: string;
    status: string;
    dateCreated: string;
    range: string;
}

export interface PagedItemTrackingApplicationResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    customerCode?: string;
    postalCode?: string;
    postalDesc?: string;
    productCode?: string;
    productDesc?: string;
    status?: string;
    dateCreated?: string;
    range?: string;
}
