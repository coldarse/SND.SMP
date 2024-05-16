import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface CustomerPostalDto extends EntityDto<number> {
    postal: string;
    rate: number;
    accountNo: number;
}

export interface PagedCustomerPostalResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    postal?: string;
    rate?: number;
    accountNo?: number;
}

export interface DetailedCustomerPostalDto extends EntityDto<number>  {
    postal: string;
    rate: number;
    rateCard: string;
    accountNo: number;
    code: string;
    createWallet: CreateWalletDto;
}

export interface CreateWalletDto {
    exists: boolean;
    create?: boolean;
    customer?: string;
    ewalletType?: number;
    currency?: number;
    currencyDesc?: string;
    balance?: number;
}
