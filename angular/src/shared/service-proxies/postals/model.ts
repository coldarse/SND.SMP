import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface PostalDto extends EntityDto<number> {
    postalCode: string;
    postalDesc: string;
    serviceCode: string;
    serviceDesc: string;
    productCode: string;
    productDesc: string;
    itemTopUpValue: number;
}

export interface PagedPostalResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    postalCode?: string;
    postalDesc?: string;
    serviceCode?: string;
    serviceDesc?: string;
    productCode?: string;
    productDesc?: string;
}

export interface PostalDDL {
    postalCode: string;
    postalDesc: string;
}

