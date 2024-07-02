import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface EmailContentDto extends EntityDto<number> {
    name: string;
    subject: string;
    content: string;
}

export interface PagedEmailContentResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    name?: string;
    subject?: string;
    content?: string;
}
