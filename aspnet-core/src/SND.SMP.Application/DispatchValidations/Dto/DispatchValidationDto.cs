using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.DispatchValidations.Dto
{

    [AutoMap(typeof(DispatchValidation))]
    public class DispatchValidationDto : EntityDto<string>
    {
        public string CustomerCode { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string DispatchNo { get; set; }
        public string FilePath { get; set; }
        public bool? IsFundLack { get; set; }
        public bool? IsValid { get; set; }
        public string PostalCode { get; set; }
        public string ServiceCode { get; set; }
        public string ProductCode { get; set; }
        public string Status { get; set; }
        public double? TookInSec { get; set; }
        public int? ValidationProgress { get; set; }
    }
}
