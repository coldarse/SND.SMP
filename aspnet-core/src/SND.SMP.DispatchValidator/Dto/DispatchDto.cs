using System;

namespace SND.SMP.DispatchValidator.Dto
{
    public class DispatchProfileDto
    {
        public string DispatchNo { get; set; } = "";
        public string AccNo { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string ServiceCode { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public DateOnly DateDispatch { get; set; }
        public string RateOptionId { get; set; } = "";
        public string PaymentMode { get; set; }
        public bool IsValid { get; set; }
    }

    public class DispatchItemDto
    {
        public string PostalCode { get; set; } = "";
        public DateOnly DispatchDate { get; set; }
        public string ServiceCode { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string BagNo { get; set; } = "";
        public string CountryCode { get; set; } = "";
        public decimal Weight { get; set; }
        public string TrackingNo { get; set; } = "";
        public string SealNo { get; set; } = "";
        public string DispatchNo { get; set; } = "";
        public decimal ItemValue { get; set; }
        public string ItemDesc { get; set; } = "";
        public string RecipientName { get; set; } = "";
        public string TelNo { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string City { get; set; } = "";
        public string AddressLine2 { get; set; } = "";
        public string AddressNo { get; set; } = "";
        public string IdentityNo { get; set; } = "";
        public string IdentityType { get; set; } = "";
        public string State { get; set; } = "";
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string TaxPaymentMethod { get; set; } = "";
        public string HSCode { get; set; } = "";
        public int Qty { get; set; }

        public decimal? Price { get; set; }
    }
}

