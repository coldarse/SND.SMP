import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';

export interface RateDto extends EntityDto<number> {
    cardName: string;
    count: number;
    service: string;
}

export interface PagedRatesResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    cardName?: string;
    count?: number
}

export interface RateDDL {
    id: number;
    cardName: string;
}
