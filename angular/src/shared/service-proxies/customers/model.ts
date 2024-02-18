import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface CustomerDto extends EntityDto<number> {
    code: string;
    companyName: string;
    emailAddress: string;
    password: string;
    addressLine1: string;
    addressLine2: string;
    city: string;
    state: string;
    country: string;
    phoneNumber: string;
    registrationNo: string;
    emailAddress2: string;
    emailAddress3: string;
    isActive: boolean;
}

export interface PagedCustomerResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    code?: string;
    companyName?: string;
    emailAddress?: string;
    password?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    state?: string;
    country?: string;
    phoneNumber?: string;
    registrationNo?: string;
    emailAddress2?: string;
    emailAddress3?: string;
    isActive?: boolean;
}

export interface CompanyNameAndCode {
    name: string;
    code: string;
}
