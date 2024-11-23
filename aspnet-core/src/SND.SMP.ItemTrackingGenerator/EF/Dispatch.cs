using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingGenerator.EF;

public partial class Dispatch
{
    public uint Id { get; set; }

    public string CustomerCode { get; set; }

    public string Pobox { get; set; }

    public string Ppi { get; set; }

    public string PostalCode { get; set; }

    public string ServiceCode { get; set; }

    public DateOnly? DispatchDate { get; set; }

    public string ProductCode { get; set; }

    public string DispatchNo { get; set; }

    public DateOnly? EtatoHkg { get; set; }

    public string FlightTrucking { get; set; }

    public string BatchId { get; set; }

    public ulong? IsPayment { get; set; }

    public int? NoofBag { get; set; }

    public int? ItemCount { get; set; }

    public decimal? TotalWeight { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? Status { get; set; }

    public ulong? IsActive { get; set; }

    public string Cn38 { get; set; }

    public DateTime? TransactionDateTime { get; set; }

    public DateTime? Ata { get; set; }

    public int? PostCheckTotalBags { get; set; }

    public decimal? PostCheckTotalWeight { get; set; }

    public int? AirportHandling { get; set; }

    public string Remark { get; set; }

    public decimal? WeightGap { get; set; }

    public decimal? WeightAveraged { get; set; }

    public DateTime? DateSoaprocessCompleted { get; set; }

    public int? SoaprocessCompletedById { get; set; }

    public decimal? TotalWeightSoa { get; set; }

    public decimal? TotalAmountSoa { get; set; }

    public int? PerformanceDaysDiff { get; set; }

    public DateTime? DatePerformanceDaysDiff { get; set; }

    public string AirlineCode { get; set; }

    public string FlightNo { get; set; }

    public string PortDeparture { get; set; }

    public string ExtDispatchNo { get; set; }

    public DateTime? DateFlight { get; set; }

    public string AirportTranshipment { get; set; }

    public string OfficeDestination { get; set; }

    public string OfficeOrigin { get; set; }

    public string Stage1StatusDesc { get; set; }

    public string Stage2StatusDesc { get; set; }

    public string Stage3StatusDesc { get; set; }

    public string Stage4StatusDesc { get; set; }

    public string Stage5StatusDesc { get; set; }

    public string Stage6StatusDesc { get; set; }

    public string Stage7StatusDesc { get; set; }

    public string Stage8StatusDesc { get; set; }

    public string Stage9StatusDesc { get; set; }

    public DateTime? DateStartedApi { get; set; }

    public DateTime? DateEndedApi { get; set; }

    public string StatusApi { get; set; }

    public string CountryOfLoading { get; set; }

    public DateTime? DateFlightArrival { get; set; }

    public ulong? PostManifestSuccess { get; set; }

    public string PostManifestMsg { get; set; }

    public DateTime? PostManifestDate { get; set; }

    public ulong? PostDeclarationSuccess { get; set; }

    public string PostDeclarationMsg { get; set; }

    public DateTime? PostDeclarationDate { get; set; }

    public string AirwayBlno { get; set; }

    public DateTime? AirwayBldate { get; set; }

    public DateTime? DateLocalDelivery { get; set; }

    public DateTime? DateClstage1Submitted { get; set; }

    public DateTime? DateClstage4Submitted { get; set; }

    public DateTime? DateClstage6Submitted { get; set; }

    public string Brcn38requestId { get; set; }

    public DateTime? DateClstage2Submitted { get; set; }

    public DateTime? DateClstage3Submitted { get; set; }

    public DateTime? DateClstage5Submitted { get; set; }

    public DateTime? DateArrival { get; set; }

    public DateTime? DateAcceptanceScanning { get; set; }

    public int? SeqNo { get; set; }

    public string CorateOptionId { get; set; }

    public string PaymentMode { get; set; }

    public string CurrencyId { get; set; }

    public int? ImportProgress { get; set; }

    public virtual ICollection<Bag> Bags { get; set; } = new List<Bag>();

    public virtual ICollection<Itemmin> Itemmins { get; set; } = new List<Itemmin>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
