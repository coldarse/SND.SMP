import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface QueueDto extends EntityDto<number> {
    eventType: string;
    filePath: string;
    deleteFileOnSuccess: boolean;
    deleteFileOnFailed: boolean;
    dateCreated: string;
    status: string;
    tookInSec: number;
    errorMsg: string;
    startTime: string;
    endTime: string;
}

export interface PagedQueueResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    eventType?: string;
    filePath?: string;
    deleteFileOnSuccess?: boolean;
    deleteFileOnFailed?: boolean;
    dateCreated?: string;
    status?: string;
    errorMsg?: string;
    startTime?: string;
    endTime?: string;
}
