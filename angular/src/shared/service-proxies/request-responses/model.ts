import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface RequestResponseDto extends EntityDto<number> {
    url: string;
    requestBody: string;
    responseBody: string;
    requestDateTime?: Date;
    responseDateTime?: Date;
    duration?: number;
}

export interface PagedRequestResponseResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    url?: string;
    requestBody?: string;
    responseBody?: string;
    requestDateTime?: Date;
    responseDateTime?: Date;
    duration?: number;
}
