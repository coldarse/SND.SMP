import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ItemMinDto extends EntityDto<number> {
    extID                         : string;
    dispatchID                    : number;
    bagID                         : number;
    month                         : number;
    countryCode                   : string;
    recpName                      : string;
    itemDesc                      : string;
    address                       : string;
    city                          : string;
    telNo                         : string;
    deliveredInDays               : number;
    isDelivered                   : boolean;
    status                        : number;
}

export interface PagedItemMinResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    extID                         ?: string;
    countryCode                   ?: string;
    recpName                      ?: string;
    itemDesc                      ?: string;
    address                       ?: string;
    city                          ?: string;
    telNo                         ?: string;
}
