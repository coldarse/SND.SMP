using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Items.Dto;
using SND.SMP.ItemTrackings;
using SND.SMP.ApplicationSettings;

namespace SND.SMP.Items
{
    public class ItemAppService(
        IRepository<Item, string> repository,
        IRepository<ItemTracking, int> itemTrackingRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository 
    ) : AsyncCrudAppService<Item, ItemDto, string, PagedItemResultRequestDto>(repository)
    {
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        protected override IQueryable<Item> CreateFilteredQuery(PagedItemResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.ExtID                         .Contains(input.Keyword) ||
                    x.PostalCode                    .Contains(input.Keyword) ||
                    x.ServiceCode                   .Contains(input.Keyword) ||
                    x.ProductCode                   .Contains(input.Keyword) ||
                    x.CountryCode                   .Contains(input.Keyword) ||
                    x.BagNo                         .Contains(input.Keyword) ||
                    x.SealNo                        .Contains(input.Keyword) ||
                    x.ItemDesc                      .Contains(input.Keyword) ||
                    x.RecpName                      .Contains(input.Keyword) ||
                    x.TelNo                         .Contains(input.Keyword) ||
                    x.Email                         .Contains(input.Keyword) ||
                    x.Address                       .Contains(input.Keyword) ||
                    x.Postcode                      .Contains(input.Keyword) ||
                    x.RateCategory                  .Contains(input.Keyword) ||
                    x.City                          .Contains(input.Keyword) ||
                    x.Address2                      .Contains(input.Keyword) ||
                    x.AddressNo                     .Contains(input.Keyword) ||
                    x.State                         .Contains(input.Keyword) ||
                    x.HSCode                        .Contains(input.Keyword) ||
                    x.PassportNo                    .Contains(input.Keyword) ||
                    x.TaxPayMethod                  .Contains(input.Keyword) ||
                    x.Stage6OMTStatusDesc           .Contains(input.Keyword) ||
                    x.Stage6OMTDestinationCity      .Contains(input.Keyword) ||
                    x.Stage6OMTDestinationCityCode  .Contains(input.Keyword) ||
                    x.Stage6OMTCountryCode          .Contains(input.Keyword) ||
                    x.ExtMsg                        .Contains(input.Keyword) ||
                    x.IdentityType                  .Contains(input.Keyword) ||
                    x.SenderName                    .Contains(input.Keyword) ||
                    x.IOSSTax                       .Contains(input.Keyword) ||
                    x.RefNo                         .Contains(input.Keyword) ||
                    x.ExemptedRemark                .Contains(input.Keyword) ||
                    x.CLCuartel                     .Contains(input.Keyword) ||
                    x.CLSector                      .Contains(input.Keyword) ||
                    x.CLSDP                         .Contains(input.Keyword) ||
                    x.CLCodigoDelegacionDestino     .Contains(input.Keyword) ||
                    x.CLNombreDelegacionDestino     .Contains(input.Keyword) ||
                    x.CLDireccionDestino            .Contains(input.Keyword) ||
                    x.CLCodigoEncaminamiento        .Contains(input.Keyword) ||
                    x.CLNumeroEnvio                 .Contains(input.Keyword) ||
                    x.CLComunaDestino               .Contains(input.Keyword) ||
                    x.CLAbreviaturaServicio         .Contains(input.Keyword) ||
                    x.CLAbreviaturaCentro           .Contains(input.Keyword) ||
                    x.Stage1StatusDesc              .Contains(input.Keyword) ||
                    x.Stage2StatusDesc              .Contains(input.Keyword) ||
                    x.Stage3StatusDesc              .Contains(input.Keyword) ||
                    x.Stage4StatusDesc              .Contains(input.Keyword) ||
                    x.Stage5StatusDesc              .Contains(input.Keyword) ||
                    x.Stage6StatusDesc              .Contains(input.Keyword) ||
                    x.Stage7StatusDesc              .Contains(input.Keyword) ||
                    x.Stage8StatusDesc              .Contains(input.Keyword) ||
                    x.Stage9StatusDesc              .Contains(input.Keyword) ||
                    x.CityId                        .Contains(input.Keyword) ||
                    x.FinalOfficeId                 .Contains(input.Keyword));
        }
    
        
    }
}
