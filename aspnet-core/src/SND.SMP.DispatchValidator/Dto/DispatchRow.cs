public class DispatchRow
{
    public int Row { get; set; } = 0;
    public string PostalCode { get; set; } = string.Empty;
    public DateOnly DispatchDate { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string BagNo { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string SealNo { get; set; } = string.Empty;
    public string DispatchNo { get; set; } = string.Empty;
    public decimal ItemValue { get; set; }
    public string ItemDesc { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string AddressNo { get; set; } = string.Empty;
    public string IdentityNo { get; set; } = string.Empty;
    public string IdentityType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string TaxPaymentMethod { get; set; } = string.Empty;
    public string HSCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}