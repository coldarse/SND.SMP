using System;
using static SND.SMP.ItemTrackingUpdater.RateWeightBreakUtil;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingUpdater.Dto
{
    public class RateCardWeightBreakDto
    {
        public string RateCardName { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Postal { get; set; } = "";
        public string PaymentMode { get; set; } = "";
        public List<WeightBreakDto> WeightBreaks { get; set; } = new List<WeightBreakDto>();
    }

    public class RateCardNameGroupDto
    {
        public string RateCardName { get; set; } = "";
    }
}

