﻿using System;
using System.Collections.Generic;

namespace SND.SMP.RateWeightBreaks.Dto
{
    public class RateCardWeightBreakDto
    {
        public string RateCardName { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Postal { get; set; } = "";
        public string PaymentMode { get; set; } = "";
        public List<WeightBreakDto> WeightBreaks { get; set; } = [];
    }

    public class RateCardWeightBreakDisplayDto
    {
        public string RateCardName { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Postal { get; set; } = "";
        public string PaymentMode { get; set; } = "";
        public string WeightBreaks { get; set; } = "";
        public List<string> Products { get; set; } = [];
    }
}

