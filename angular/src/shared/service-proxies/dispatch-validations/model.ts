import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface DispatchValidationDto extends EntityDto<number> {
    customerCode                    : string;
    dateStarted                     : string;
    dateCompleted                   : string;
    dispatchNo                      : string;
    filePath                        : string;
    isFundLack                      : boolean;
    isValid                         : boolean;
    postalCode                      : string;
    serviceCode                     : string;
    productCode                     : string;
    status                          : string;
    tookInSec                       : number;
    validationProgress              : number;
}

export interface PagedDispatchValidationResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    customerCode                    ?: string;
    dateStarted                     ?: string;
    dateCompleted                   ?: string;
    dispatchNo                      ?: string;
    filePath                        ?: string;
    postalCode                      ?: string;
    serviceCode                     ?: string;
    productCode                     ?: string;
    status                          ?: string;
    isAdmin?: boolean;
}

export interface DispatchValidateDto {
    category: string;
    itemIds: string[];
    message: string;
}

