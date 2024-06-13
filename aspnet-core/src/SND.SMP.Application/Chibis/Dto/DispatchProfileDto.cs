using System;

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