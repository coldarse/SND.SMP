import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';
import type { EWalletTypeDto } from '../ewallettypes/model';
import type { CurrencyDto } from '../currencies/model';

export interface WalletDto extends EntityDto<string> {
    customer: string;
    eWalletType: number;
    currency: number;
    balance: number;
}

export interface PagedWalletsResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    customer?: string;
    eWalletType?: number;
    currency?: number;
    balance?: number;
}

export interface EWalletDto extends EntityDto<string> {
    customer: string;
    eWalletType: number;
    eWalletTypeDesc: string;
    currency: number;
    currencyDesc: string;
    eWalletTypeList: EWalletTypeDto[];
    currencyList: CurrencyDto[];
    balance: number;
}

export interface UpdateWalletDto {
    id: string;
    customer: string;
    eWalletType: number;
    currency: number;
    ogCustomer: string;
    ogeWalletType: number;
    ogCurrency: number;
}

export interface TopUpEWalletDto {
    eWalletType: string;
    currency: string;
    referenceNo: string;
    description: string;
    amount: number;
    id: string;
}
