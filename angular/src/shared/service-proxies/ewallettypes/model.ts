import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface EWalletTypeDto extends EntityDto<number> {
    type: string;
}

export interface PagedEWalletTypeResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    type?: string;
}
