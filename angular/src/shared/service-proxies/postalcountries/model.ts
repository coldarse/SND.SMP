import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface PostalCountryDto extends EntityDto<number> {
    postalCode: string;
    countryCode: string;
}

export interface PagedPostalCountryResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    postalCode?: string;
    countryCode?: string;
}
