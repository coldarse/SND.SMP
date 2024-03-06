using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Postals.Dto
{

    [AutoMap(typeof(Postal))]
    public class PostalDto : EntityDto<long>
    {
        public string PostalCode { get; set; }
        public string PostalDesc { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDesc { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public decimal ItemTopUpValue { get; set; }
    }
}
