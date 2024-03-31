import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface BagDto extends EntityDto<number> {
    bagNo                         : string;
    dispatchId                    : number;
    countryCode                   : string;
    itemCountPre                  : number;
    itemCountPost                 : number;
    cN35No                        : string;
}

export interface PagedBagResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    bagNo                         ?: string;
    countryCode                   ?: string;
    cN35No                        ?: string;
}
