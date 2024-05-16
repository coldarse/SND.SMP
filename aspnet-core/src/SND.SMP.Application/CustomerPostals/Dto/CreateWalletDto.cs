public class CreateWalletDto
{
    public bool Exists { get; set; }
    public bool? Create { get; set; }
    public string? Customer { get; set; }
    public long? EWalletType { get; set; }
    public long? Currency { get; set; }
    public string? CurrencyDesc { get; set; }
    public decimal? Balance { get; set; }
}