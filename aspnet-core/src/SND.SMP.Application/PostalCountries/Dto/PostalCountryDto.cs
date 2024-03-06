using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.PostalCountries.Dto
{

    [AutoMap(typeof(PostalCountry))]
    public class PostalCountryDto : EntityDto<long>
    {
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }
}
