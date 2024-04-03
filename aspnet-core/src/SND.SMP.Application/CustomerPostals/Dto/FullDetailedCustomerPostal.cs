using System.Collections.Generic;
using Abp.Application.Services.Dto;

public class FullDetailedCustomerPostal
{
    public PagedResultDto<DetailedCustomerPostalDto> PagedResultDto { get; set; }
    public List<PostalDDL> PostalDDLs { get; set; }
    public List<RateDDL> RateDDLs { get; set; }
}