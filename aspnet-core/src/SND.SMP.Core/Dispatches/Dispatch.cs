using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Dispatches
{

    public class Dispatch : Entity<int>
    {
        public string CustomerCode                     { get; set; }
        public string POBox                            { get; set; }
        public string PPI                              { get; set; }
        public string PostalCode                       { get; set; }
        public string ServiceCode                      { get; set; }
        public string ProductCode                      { get; set; }
        public DateOnly? DispatchDate                     { get; set; }
        public string DispatchNo                       { get; set; }
        public DateOnly? ETAtoHKG                         { get; set; }
        public string FlightTrucking                   { get; set; }
        public string BatchId                          { get; set; }
        public bool? IsPayment                        { get; set; }
        public int? NoofBag                          { get; set; } = 0;
        public int? ItemCount                        { get; set; } = 0;
        public decimal? TotalWeight                      { get; set; }
        public decimal? TotalPrice                       { get; set; }
        public int? Status                           { get; set; } = 0;
        public bool? IsActive                         { get; set; }
        public string CN38                             { get; set; }
        public DateTime? TransactionDateTime              { get; set; }
        public DateTime? ATA                              { get; set; }
        public int? PostCheckTotalBags               { get; set; } = 0;
        public decimal? PostCheckTotalWeight             { get; set; }
        public int? AirportHandling                  { get; set; } = 0;
        public string Remark                           { get; set; }
        public decimal? WeightGap                        { get; set; }
        public decimal? WeightAveraged                   { get; set; }
        public DateTime? DateSOAProcessCompleted          { get; set; }
        public int? SOAProcessCompletedByID          { get; set; } = 0;
        public decimal? TotalWeightSOA                   { get; set; }
        public decimal? TotalAmountSOA                   { get; set; }
        public int? PerformanceDaysDiff              { get; set; } = 0;
        public DateTime? DatePerformanceDaysDiff          { get; set; }
        public string AirlineCode                      { get; set; }
        public string FlightNo                         { get; set; }
        public string PortDeparture                    { get; set; }
        public string ExtDispatchNo                    { get; set; }
        public DateTime? DateFlight                       { get; set; }
        public string AirportTranshipment              { get; set; }
        public string OfficeDestination                { get; set; }
        public string OfficeOrigin                     { get; set; }
        public string Stage1StatusDesc                 { get; set; }
        public string Stage2StatusDesc                 { get; set; }
        public string Stage3StatusDesc                 { get; set; }
        public string Stage4StatusDesc                 { get; set; }
        public string Stage5StatusDesc                 { get; set; }
        public string Stage6StatusDesc                 { get; set; }
        public string Stage7StatusDesc                 { get; set; }
        public string Stage8StatusDesc                 { get; set; }
        public string Stage9StatusDesc                 { get; set; }
        public DateTime? DateStartedAPI                   { get; set; }
        public DateTime? DateEndedAPI                     { get; set; }
        public string StatusAPI                        { get; set; }
        public string CountryOfLoading                 { get; set; }
        public DateTime? DateFlightArrival                { get; set; }
        public bool? PostManifestSuccess              { get; set; }
        public string PostManifestMsg                  { get; set; }
        public DateTime? PostManifestDate                 { get; set; }
        public bool? PostDeclarationSuccess           { get; set; }
        public string PostDeclarationMsg               { get; set; }
        public DateTime? PostDeclarationDate              { get; set; }
        public string AirwayBLNo                       { get; set; }
        public DateTime? AirwayBLDate                     { get; set; }
        public DateTime? DateLocalDelivery                { get; set; }
        public DateTime? DateCLStage1Submitted            { get; set; }
        public DateTime? DateCLStage2Submitted            { get; set; }
        public DateTime? DateCLStage3Submitted            { get; set; }
        public DateTime? DateCLStage4Submitted            { get; set; }
        public DateTime? DateCLStage5Submitted            { get; set; }
        public DateTime? DateCLStage6Submitted            { get; set; }
        public string BRCN38RequestId                  { get; set; }
        public DateTime? DateArrival                      { get; set; }
        public DateTime? DateAcceptanceScanning           { get; set; }
        public int? SeqNo                            { get; set; } = 0;
        public string CORateOptionId                   { get; set; }
        public string PaymentMode                      { get; set; }
        public string CurrencyId                       { get; set; }
        public int? ImportProgress                   { get; set; } = 0;
    }
}
