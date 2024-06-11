import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface AirportDto extends EntityDto<number> {
    name: string;
    code: string;
    country: string;
}

export interface PagedAirportResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    name?: string;
    code?: string;
    country?: string;
}
