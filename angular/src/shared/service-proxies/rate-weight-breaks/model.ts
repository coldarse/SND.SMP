import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RateWeightBreakDto extends EntityDto<number> {
    rateId: number;
    postalOrgId: string;
    weightMin: number;
    weightMax: number;
    productCode: string;
    currencyId: number;
    itemRate: number;
    weightRate: number;
    isExceedRule: boolean;
    paymentMode: string;
}

export interface PagedRateWeightBreakResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    postalOrgId?: string;
    productCode?: string;
    isExceedRule?: boolean;
    paymentMode?: string;
}
