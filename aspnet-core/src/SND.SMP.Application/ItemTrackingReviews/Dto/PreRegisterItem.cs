using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class InPreRegisterItem : PreRegisterItem
{
    public string HSCode { get; set; }
    public string SenderName { get; set; }
    public string IOSSTax { get; set; }
    public string AddressNo { get; set; }
    public string IdentityType { get; set; }
    public string IdentityNo { get; set; }
    public string? PostOfficeName { get; set; }
}

public class PreRegisterItem
{
    [Required]
    public string ClientKey { get; set; }
    [Required]
    public string RefNo { get; set; }
    [Required]
    public string SignatureHash { get; set; }
    [Required]
    public string ItemID { get; set; }
    [Required]
    public string PostalCode { get; set; }
    [Required]
    public string ServiceCode { get; set; }
    [Required]
    public string ProductCode { get; set; }
    [Required]
    public string RecipientName { get; set; }
    [Required]
    public string RecipientContactNo { get; set; }
    public string RecipientEmail { get; set; }
    [Required]
    public string RecipientAddress { get; set; }
    [Required]
    public string RecipientCity { get; set; }
    [Required]
    public string RecipientState { get; set; }
    [Required]
    public string RecipientPostcode { get; set; }
    [Required]
    public string RecipientCountry { get; set; }
    [Required]
    public decimal Weight { get; set; }
    [Required]
    public decimal ItemValue { get; set; }
    [Required]
    public string ItemDesc { get; set; }
    public string PoolItemId { get; set; }
}

public class OutPreRegisterItem
{
    public string Status { get; set; }
    public List<string> Errors { get; set; }
    public string RefNo { get; set; }
    public string ResponseID { get; set; }
    public string ItemID { get; set; }
    public string APIItemID { get; set; }
    public string SignatureHash { get; set; }
    public string Destino { get; set; }
    public string Remarks { get; set; }
}