import type { PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';


export interface ItemDto extends EntityDto<number> {
    extID                         : string;
    dispatchID                    : number;
    bagID                         : number;
    month                         : number;
    postalCode                    : string;
    serviceCode                   : string;
    productCode                   : string;
    countryCode                   : string;
    bagNo                         : string;
    sealNo                        : string;
    status                        : number;
    itemDesc                      : string;
    recpName                      : string;
    telNo                         : string;
    email                         : string;
    address                       : string;
    postcode                      : string;
    rateCategory                  : string;
    city                          : string;
    address2                      : string;
    addressNo                     : string;
    state                         : string;
    hSCode                        : string;
    qty                           : number;
    passportNo                    : string;
    taxPayMethod                  : string;
    dateStage1                    : string;
    dateStage2                    : string;
    dateStage3                    : string;
    dateStage4                    : string;
    dateStage5                    : string;
    dateStage6                    : string;
    dateStage7                    : string;
    dateStage8                    : string;
    dateStage9                    : string;
    stage6OMTStatusDesc           : string;
    stage6OMTDepartureDate        : string;
    stage6OMTArrivalDate          : string;
    stage6OMTDestinationCity      : string;
    stage6OMTDestinationCityCode  : string;
    stage6OMTCountryCode          : string;
    extMsg                        : string;
    identityType                  : string;
    senderName                    : string;
    iOSSTax                       : string;
    refNo                         : string;
    dateSuccessfulDelivery        : string;
    isDelivered                   : boolean;
    deliveredInDays               : number;
    isExempted                    : boolean;
    exemptedRemark                : string;
    cLCuartel                     : string;
    cLSector                      : string;
    cLSDP                         : string;
    cLCodigoDelegacionDestino     : string;
    cLNombreDelegacionDestino     : string;
    cLDireccionDestino            : string;
    cLCodigoEncaminamiento        : string;
    cLNumeroEnvio                 : string;
    cLComunaDestino               : string;
    cLAbreviaturaServicio         : string;
    cLAbreviaturaCentro           : string;
    stage1StatusDesc              : string;
    stage2StatusDesc              : string;
    stage3StatusDesc              : string;
    stage4StatusDesc              : string;
    stage5StatusDesc              : string;
    stage6StatusDesc              : string;
    stage7StatusDesc              : string;
    stage8StatusDesc              : string;
    stage9StatusDesc              : string;
    cityId                        : string;
    finalOfficeId                 : string;
}

export interface PagedItemResultRequestDto extends PagedAndSortedResultRequestDto {
    keyword?: string;
    extID                         ?: string;
    postalCode                    ?: string;
    serviceCode                   ?: string;
    productCode                   ?: string;
    countryCode                   ?: string;
    bagNo                         ?: string;
    sealNo                        ?: string;
    itemDesc                      ?: string;
    recpName                      ?: string;
    telNo                         ?: string;
    email                         ?: string;
    address                       ?: string;
    postcode                      ?: string;
    rateCategory                  ?: string;
    city                          ?: string;
    address2                      ?: string;
    addressNo                     ?: string;
    state                         ?: string;
    hSCode                        ?: string;
    passportNo                    ?: string;
    taxPayMethod                  ?: string;
    dateStage1                    ?: string;
    dateStage2                    ?: string;
    dateStage3                    ?: string;
    dateStage4                    ?: string;
    dateStage5                    ?: string;
    dateStage6                    ?: string;
    dateStage7                    ?: string;
    dateStage8                    ?: string;
    dateStage9                    ?: string;
    stage6OMTStatusDesc           ?: string;
    stage6OMTDepartureDate        ?: string;
    stage6OMTArrivalDate          ?: string;
    stage6OMTDestinationCity      ?: string;
    stage6OMTDestinationCityCode  ?: string;
    stage6OMTCountryCode          ?: string;
    extMsg                        ?: string;
    identityType                  ?: string;
    senderName                    ?: string;
    iOSSTax                       ?: string;
    refNo                         ?: string;
    dateSuccessfulDelivery        ?: string;
    exemptedRemark                ?: string;
    cLCuartel                     ?: string;
    cLSector                      ?: string;
    cLSDP                         ?: string;
    cLCodigoDelegacionDestino     ?: string;
    cLNombreDelegacionDestino     ?: string;
    cLDireccionDestino            ?: string;
    cLCodigoEncaminamiento        ?: string;
    cLNumeroEnvio                 ?: string;
    cLComunaDestino               ?: string;
    cLAbreviaturaServicio         ?: string;
    cLAbreviaturaCentro           ?: string;
    stage1StatusDesc              ?: string;
    stage2StatusDesc              ?: string;
    stage3StatusDesc              ?: string;
    stage4StatusDesc              ?: string;
    stage5StatusDesc              ?: string;
    stage6StatusDesc              ?: string;
    stage7StatusDesc              ?: string;
    stage8StatusDesc              ?: string;
    stage9StatusDesc              ?: string;
    cityId                        ?: string;
    finalOfficeId                 ?: string;
}
