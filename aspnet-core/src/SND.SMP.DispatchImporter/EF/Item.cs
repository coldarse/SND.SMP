using System;
using System.Collections.Generic;

namespace SND.SMP.DispatchImporter.EF;

public partial class Item
{
    public string Id { get; set; }

    public string ExtId { get; set; }

    public uint? DispatchId { get; set; }

    public uint? BagId { get; set; }

    public DateOnly? DispatchDate { get; set; }

    public int? Month { get; set; }

    public string PostalCode { get; set; }

    public string ServiceCode { get; set; }

    public string ProductCode { get; set; }

    public string CountryCode { get; set; }

    public decimal? Weight { get; set; }

    public string BagNo { get; set; }

    public string SealNo { get; set; }

    public decimal? Price { get; set; }

    public int? Status { get; set; }

    public decimal? ItemValue { get; set; }

    public string ItemDesc { get; set; }

    public string RecpName { get; set; }

    public string TelNo { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public string Postcode { get; set; }

    public string RateCategory { get; set; }

    public string City { get; set; }

    public string Address2 { get; set; }

    public string AddressNo { get; set; }

    public string State { get; set; }

    public decimal? Length { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public string Hscode { get; set; }

    public int? Qty { get; set; }

    public string PassportNo { get; set; }

    public string TaxPayMethod { get; set; }

    public DateTime? DateStage1 { get; set; }

    public DateTime? DateStage2 { get; set; }

    public DateTime? DateStage3 { get; set; }

    public DateTime? DateStage4 { get; set; }

    public DateTime? DateStage5 { get; set; }

    public DateTime? DateStage6 { get; set; }

    public DateTime? DateStage7 { get; set; }

    public DateTime? DateStage8 { get; set; }

    public DateTime? DateStage9 { get; set; }

    public string Stage6OmtstatusDesc { get; set; }

    public DateTime? Stage6OmtdepartureDate { get; set; }

    public DateTime? Stage6OmtarrivalDate { get; set; }

    public string Stage6OmtdestinationCity { get; set; }

    public string Stage6OmtdestinationCityCode { get; set; }

    public string Stage6OmtcountryCode { get; set; }

    public string ExtMsg { get; set; }

    public string IdentityType { get; set; }

    public string SenderName { get; set; }

    public string Iosstax { get; set; }

    public string RefNo { get; set; }

    public DateTime? DateSuccessfulDelivery { get; set; }

    public ulong? IsDelivered { get; set; }

    public int? DeliveredInDays { get; set; }

    public ulong? IsExempted { get; set; }

    public string ExemptedRemark { get; set; }

    public string Clcuartel { get; set; }

    public string Clsector { get; set; }

    public string Clsdp { get; set; }

    public string ClcodigoDelegacionDestino { get; set; }

    public string ClnombreDelegacionDestino { get; set; }

    public string CldireccionDestino { get; set; }

    public string ClcodigoEncaminamiento { get; set; }

    public string ClnumeroEnvio { get; set; }

    public string ClcomunaDestino { get; set; }

    public string ClabreviaturaServicio { get; set; }

    public string ClabreviaturaCentro { get; set; }

    public string Stage1StatusDesc { get; set; }

    public string Stage2StatusDesc { get; set; }

    public string Stage3StatusDesc { get; set; }

    public string Stage4StatusDesc { get; set; }

    public string Stage5StatusDesc { get; set; }

    public string Stage6StatusDesc { get; set; }

    public string Stage7StatusDesc { get; set; }

    public string Stage8StatusDesc { get; set; }

    public string Stage9StatusDesc { get; set; }

    public string CityId { get; set; }

    public string FinalOfficeId { get; set; }

    public virtual Bag Bag { get; set; }

    public virtual Dispatch Dispatch { get; set; }
}
