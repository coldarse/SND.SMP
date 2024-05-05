import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface IMPCDto extends EntityDto<number> {
    type: string;
    countryCode: string;
    airportCode: string;
    iMPCCode: string;
    logisticCode: string;
}

export interface PagedIMPCResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    type?: string;
    countryCode?: string;
    airportCode?: string;
    iMPCCode?: string;
    logisticCode?: string;
}
