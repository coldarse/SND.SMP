import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ApplicationSettingDto extends EntityDto<number> {
    name: string;
    value: string;
}

export interface PagedApplicationSettingResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    name?: string;
    value?: string;
}
