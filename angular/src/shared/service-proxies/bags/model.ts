import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface BagDto extends EntityDto<number> {
    bagNo                         : string;
    dispatchId                    : number;
    countryCode                   : string;
    weightPre                     : number;
    weightPost                    : number;
    itemCountPre                  : number;
    itemCountPost                 : number;
    weightVariance                : number;
    cN35No                        : string;
    underAmount                   : number;
}

export interface PagedBagResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    bagNo                         ?: string;
    countryCode                   ?: string;
    cN35No                        ?: string;
}
