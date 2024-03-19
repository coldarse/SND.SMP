using Abp.Application.Services.Dto;

public class DetailedCustomerPostalDto : EntityDto<long>
{
    public string Postal { get; set; }
    public int Rate { get; set; }
    public string RateCard { get; set; }
    public long AccountNo { get; set; }
    public string Code { get; set; }
}