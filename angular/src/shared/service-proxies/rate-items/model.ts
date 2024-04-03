import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';

export interface RateItemDto extends EntityDto<number> {
    rateId: number;
    serviceCode: string;
    productCode: string;
    countryCode: string;
    total: number;
    fee: number;
    currencyId: number;
    paymentMode: string;
}

export interface PagedRateItemsResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    rateId?: number;
    serviceCode?: string;
    productCode?: string;
    countryCode?: string;
    total?: number;
    fee?: number;
    currencyId?: number;
    paymentMode?: string;
}

export interface RateItemDetailDto {
    id: number;
    rateId: number;
    rateCardName : number;
    serviceCode: string;
    productCode: string;
    countryCode: string;
    total: number;
    fee: number;
    currencyId: number;
    currency: string;
    paymentMode: string;
}



