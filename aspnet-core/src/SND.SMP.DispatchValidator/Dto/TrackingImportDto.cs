using System;
namespace SND.SMP.DispatchValidator.Dto
{
    public class TrackingImportDto
    {
        public string DispatchNo { get; set; }
        public string BagNo { get; set; }
        public DateTime? DateStage1 { get; set; }
        public DateTime? DateStage2 { get; set; }
        public DateTime? DateStage3 { get; set; }
        public DateTime? DateStage4 { get; set; }
        public DateTime? DateStage5 { get; set; }
        public DateTime? DateStage6 { get; set; }
        public DateTime? DateStage7 { get; set; }
        public string DestinationAirport { get; set; }
    }
}

