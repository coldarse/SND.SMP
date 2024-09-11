import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RateZoneDto extends EntityDto<number> {
    zone: string;
    state: string;
    city: string;
    postCode: string;
    country: string;
}

export interface PagedRateZoneResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    zone?: string;
    state?: string;
    city?: string;
    postCode?: string;
    country?: string;
}
