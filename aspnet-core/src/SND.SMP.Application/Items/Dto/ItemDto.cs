using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace SND.SMP.Items.Dto
{

    [AutoMap(typeof(Item))]
    public class ItemDto : EntityDto<string>
    {
        public string ExtID                          { get; set; }
        public int? DispatchID                     { get; set; }
        public int? BagID                          { get; set; }
        public DateOnly DispatchDate                   { get; set; }
        public int? Month                          { get; set; }
        public string PostalCode                     { get; set; }
        public string ServiceCode                    { get; set; }
        public string ProductCode                    { get; set; }
        public string CountryCode                    { get; set; }
        public decimal Weight                         { get; set; }
        public string BagNo                          { get; set; }
        public string SealNo                         { get; set; }
        public decimal Price                          { get; set; }
        public int? Status                         { get; set; }
        public decimal ItemValue                      { get; set; }
        public string ItemDesc                       { get; set; }
        public string RecpName                       { get; set; }
        public string TelNo                          { get; set; }
        public string Email                          { get; set; }
        public string Address                        { get; set; }
        public string Postcode                       { get; set; }
        public string RateCategory                   { get; set; }
        public string City                           { get; set; }
        public string Address2                       { get; set; }
        public string AddressNo                      { get; set; }
        public string State                          { get; set; }
        public decimal Length                         { get; set; }
        public decimal Width                          { get; set; }
        public decimal Height                         { get; set; }
        public string HSCode                         { get; set; }
        public int? Qty                            { get; set; }
        public string PassportNo                     { get; set; }
        public string TaxPayMethod                   { get; set; }
        public DateTime? DateStage1                     { get; set; }
        public DateTime? DateStage2                     { get; set; }
        public DateTime? DateStage3                     { get; set; }
        public DateTime? DateStage4                     { get; set; }
        public DateTime? DateStage5                     { get; set; }
        public DateTime? DateStage6                     { get; set; }
        public DateTime? DateStage7                     { get; set; }
        public DateTime? DateStage8                     { get; set; }
        public DateTime? DateStage9                     { get; set; }
        public string Stage6OMTStatusDesc            { get; set; }
        public DateTime? Stage6OMTDepartureDate         { get; set; }
        public DateTime? Stage6OMTArrivalDate           { get; set; }
        public string Stage6OMTDestinationCity       { get; set; }
        public string Stage6OMTDestinationCityCode   { get; set; }
        public string Stage6OMTCountryCode           { get; set; }
        public string ExtMsg                         { get; set; }
        public string IdentityType                   { get; set; }
        public string SenderName                     { get; set; }
        public string IOSSTax                        { get; set; }
        public string RefNo                          { get; set; }
        public DateTime? DateSuccessfulDelivery         { get; set; }
        public bool? IsDelivered                    { get; set; }
        public int? DeliveredInDays                { get; set; }
        public bool? IsExempted                     { get; set; }
        public string ExemptedRemark                 { get; set; }
        public string CLCuartel                      { get; set; }
        public string CLSector                       { get; set; }
        public string CLSDP                          { get; set; }
        public string CLCodigoDelegacionDestino      { get; set; }
        public string CLNombreDelegacionDestino      { get; set; }
        public string CLDireccionDestino             { get; set; }
        public string CLCodigoEncaminamiento         { get; set; }
        public string CLNumeroEnvio                  { get; set; }
        public string CLComunaDestino                { get; set; }
        public string CLAbreviaturaServicio          { get; set; }
        public string CLAbreviaturaCentro            { get; set; }
        public string Stage1StatusDesc               { get; set; }
        public string Stage2StatusDesc               { get; set; }
        public string Stage3StatusDesc               { get; set; }
        public string Stage4StatusDesc               { get; set; }
        public string Stage5StatusDesc               { get; set; }
        public string Stage6StatusDesc               { get; set; }
        public string Stage7StatusDesc               { get; set; }
        public string Stage8StatusDesc               { get; set; }
        public string Stage9StatusDesc               { get; set; }
        public string CityId                         { get; set; }
        public string FinalOfficeId                  { get; set; }
    }
}
