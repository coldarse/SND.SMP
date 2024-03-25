using System;
namespace SND.SMP.RateWeightBreaks.Dto
{
	public class WeightBreakDto
	{
        public decimal? WeightMinKg { get; set; }
        public decimal? WeightMaxKg { get; set; }
        public string ProductCode { get; set; } = "";
        public decimal? ItemRate { get; set; }
        public decimal? WeightRate { get; set; }
        public bool IsExceedRule { get; set; }
    }

    public class WeightBreakDisplayDto
	{
        public string WeightBreak { get; set; }
        public string ProductCode { get; set; } = "";
        public decimal? ItemRate { get; set; }
        public decimal? WeightRate { get; set; }
        public bool IsExceedRule { get; set; }
    }
}

