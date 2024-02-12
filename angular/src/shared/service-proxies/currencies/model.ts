import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface CurrencyDto extends EntityDto<number> {
    abbr: string;
    description: string;
}

export interface PagedCurrencyResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    abbr?: string;
    description?: string;
}
