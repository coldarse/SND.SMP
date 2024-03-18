import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface CustomerPostalDto extends EntityDto<number> {
    postal: string;
    rate: number;
    accountNo: number;
}

export interface PagedCustomerPostalResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    postal?: string;
    rate: number;
    accountNo: number;
}
