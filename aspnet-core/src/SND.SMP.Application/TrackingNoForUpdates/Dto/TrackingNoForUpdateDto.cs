using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.TrackingNoForUpdates.Dto
{

    [AutoMap(typeof(TrackingNoForUpdate))]
    public class TrackingNoForUpdateDto : EntityDto<long>
    {
        public string TrackingNo { get; set; }
        public string DispatchNo { get; set; }
        public string ProcessType { get; set; }
    }
}
