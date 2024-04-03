public class RateItemDetailDto
{
    public long Id { get; set; }
    public int RateId { get; set; }
    public string RateCardName { get; set; }
    public string ServiceCode { get; set; }
    public string ProductCode { get; set; }
    public string CountryCode { get; set; }
    public decimal Total { get; set; }
    public decimal Fee { get; set; }
    public long CurrencyId { get; set; }
    public string Currency { get; set; }
    public string PaymentMode { get; set; }
}