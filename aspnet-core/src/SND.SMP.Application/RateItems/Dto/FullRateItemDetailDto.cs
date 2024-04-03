using System.Collections.Generic;
using Abp.Application.Services.Dto;
using SND.SMP.Rates;

public class FullRateItemDetailDto
{
    public PagedResultDto<RateItemDetailDto> PagedRateItemResultDto { get; set; }
    public List<Rate> Rates { get; set; }
}