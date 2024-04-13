import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ApplicationSettingDto extends EntityDto<number> {
    name: string;
    8: string;
}

export interface PagedApplicationSettingResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    name?: string;
    8?: string;
}
