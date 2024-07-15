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
using Abp.EntityFrameworkCore.Repositories;
using SND.SMP.Dispatches;
using System.Globalization;
using SND.SMP.Postals;

namespace SND.SMP.Items
{
    public class ItemAppService(
        IRepository<Item, string> repository,
        IRepository<ItemTracking, int> itemTrackingRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<Dispatch, int> dispatchRepository,
        IRepository<Postal, long> postalRepository
    ) : AsyncCrudAppService<Item, ItemDto, string, PagedItemResultRequestDto>(repository)
    {
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<Dispatch, int> _dispatchRepository = dispatchRepository;
        private readonly IRepository<Postal, long> _postalRepository = postalRepository;
        protected override IQueryable<Item> CreateFilteredQuery(PagedItemResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.ExtID.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword) ||
                    x.BagNo.Contains(input.Keyword) ||
                    x.SealNo.Contains(input.Keyword) ||
                    x.ItemDesc.Contains(input.Keyword) ||
                    x.RecpName.Contains(input.Keyword) ||
                    x.TelNo.Contains(input.Keyword) ||
                    x.Email.Contains(input.Keyword) ||
                    x.Address.Contains(input.Keyword) ||
                    x.Postcode.Contains(input.Keyword) ||
                    x.RateCategory.Contains(input.Keyword) ||
                    x.City.Contains(input.Keyword) ||
                    x.Address2.Contains(input.Keyword) ||
                    x.AddressNo.Contains(input.Keyword) ||
                    x.State.Contains(input.Keyword) ||
                    x.HSCode.Contains(input.Keyword) ||
                    x.PassportNo.Contains(input.Keyword) ||
                    x.TaxPayMethod.Contains(input.Keyword) ||
                    x.Stage6OMTStatusDesc.Contains(input.Keyword) ||
                    x.Stage6OMTDestinationCity.Contains(input.Keyword) ||
                    x.Stage6OMTDestinationCityCode.Contains(input.Keyword) ||
                    x.Stage6OMTCountryCode.Contains(input.Keyword) ||
                    x.ExtMsg.Contains(input.Keyword) ||
                    x.IdentityType.Contains(input.Keyword) ||
                    x.SenderName.Contains(input.Keyword) ||
                    x.IOSSTax.Contains(input.Keyword) ||
                    x.RefNo.Contains(input.Keyword) ||
                    x.ExemptedRemark.Contains(input.Keyword) ||
                    x.CLCuartel.Contains(input.Keyword) ||
                    x.CLSector.Contains(input.Keyword) ||
                    x.CLSDP.Contains(input.Keyword) ||
                    x.CLCodigoDelegacionDestino.Contains(input.Keyword) ||
                    x.CLNombreDelegacionDestino.Contains(input.Keyword) ||
                    x.CLDireccionDestino.Contains(input.Keyword) ||
                    x.CLCodigoEncaminamiento.Contains(input.Keyword) ||
                    x.CLNumeroEnvio.Contains(input.Keyword) ||
                    x.CLComunaDestino.Contains(input.Keyword) ||
                    x.CLAbreviaturaServicio.Contains(input.Keyword) ||
                    x.CLAbreviaturaCentro.Contains(input.Keyword) ||
                    x.Stage1StatusDesc.Contains(input.Keyword) ||
                    x.Stage2StatusDesc.Contains(input.Keyword) ||
                    x.Stage3StatusDesc.Contains(input.Keyword) ||
                    x.Stage4StatusDesc.Contains(input.Keyword) ||
                    x.Stage5StatusDesc.Contains(input.Keyword) ||
                    x.Stage6StatusDesc.Contains(input.Keyword) ||
                    x.Stage7StatusDesc.Contains(input.Keyword) ||
                    x.Stage8StatusDesc.Contains(input.Keyword) ||
                    x.Stage9StatusDesc.Contains(input.Keyword) ||
                    x.CityId.Contains(input.Keyword) ||
                    x.FinalOfficeId.Contains(input.Keyword));
        }

        public async Task<List<APIItemIdDashboard>> GetAPIItemIdDashboard(string month)
        {
            List<APIItemIdDashboard> apiItemIdDashboard = [];

            DateOnly firstDayOfMonth = DateOnly.FromDateTime(DateTime.ParseExact(month.ToUpper(), "MMMM", CultureInfo.InvariantCulture));

            // Get last day of the month
            DateOnly lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var postals = await _postalRepository.GetAllListAsync();

            var items = await Repository.GetAllListAsync(x =>
                                        x.DispatchDate <= lastDayOfMonth &&
                                        x.DispatchDate >= firstDayOfMonth);

            var dispatches = await _dispatchRepository.GetAllListAsync(x =>
                                        x.DispatchDate <= lastDayOfMonth &&
                                        x.DispatchDate >= firstDayOfMonth);

            var distinctedCombination = (
                                from item in items
                                join dispatch in dispatches on item.DispatchID equals dispatch.Id
                                select new
                                {
                                    dispatch.CustomerCode,
                                    item.ProductCode,
                                    item.ServiceCode,
                                    item.PostalCode
                                }
                            ).Distinct().ToList();

            var joinedItems = (from item in items
                               join dispatch in dispatches on item.DispatchID equals dispatch.Id
                               select new
                               {
                                   dispatch.CustomerCode,
                                   item.ProductCode,
                                   item.ServiceCode,
                                   item.PostalCode,
                                   item.Id,
                                   item.DispatchDate
                               }).ToList();

            joinedItems = joinedItems.DistinctBy(x => x.Id).ToList();

            foreach (var distincted in distinctedCombination)
            {
                var filteredItems = joinedItems.Where(x => 
                                                        x.CustomerCode.Equals(distincted.CustomerCode) &&
                                                        x.ProductCode.Equals(distincted.ProductCode) &&
                                                        x.ServiceCode.Equals(distincted.ServiceCode) &&
                                                        x.PostalCode.Equals(distincted.PostalCode))
                                                        .OrderByDescending(x => x.DispatchDate)
                                                        .ToList();

                var postal = postals.FirstOrDefault(x => x.PostalCode.Equals(distincted.PostalCode));

                apiItemIdDashboard.Add(new APIItemIdDashboard()
                {
                    CustomerCode = distincted.CustomerCode,
                    Postal = postal is null ? distincted.PostalCode : postal.PostalDesc,
                    Service = postal is null ? distincted.ServiceCode : postal.ServiceDesc,
                    Product = postal is null ? distincted.ProductCode : postal.ProductDesc,
                    TotalItems = filteredItems.Count,
                    DateLastReceived = filteredItems[0].DispatchDate.ToString()
                });
            }


            return apiItemIdDashboard;
        }
    }
}
