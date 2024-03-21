import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RateWeightBreakDto extends EntityDto<number> {
    rateId: number;
    postalOrgId: string;
    productCode: string;
    currencyId: number;
    isExceedRule: boolean;
    paymentMode: string;
}

export interface PagedRateWeightBreakResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    rateId: number;
    postalOrgId?: string;
    productCode?: string;
    currencyId: number;
    isExceedRule?: boolean;
    paymentMode?: string;
}
