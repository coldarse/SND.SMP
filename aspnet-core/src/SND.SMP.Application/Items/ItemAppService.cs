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
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Abp.UI;
using SND.SMP.Bags;
using Microsoft.EntityFrameworkCore;
using SND.SMP.EntityFrameworkCore;

namespace SND.SMP.Items
{
    public class ItemAppService(
        IRepository<Item, string> repository,
        IRepository<ItemTracking, int> itemTrackingRepository,
        IRepository<ApplicationSetting, int> applicationSettingRepository,
        IRepository<Dispatch, int> dispatchRepository,
        IRepository<Postal, long> postalRepository,
        IRepository<Bag, int> bagRepository
    ) : AsyncCrudAppService<Item, ItemDto, string, PagedItemResultRequestDto>(repository)
    {
        private readonly IRepository<ItemTracking, int> _itemTrackingRepository = itemTrackingRepository;
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly IRepository<Dispatch, int> _dispatchRepository = dispatchRepository;
        private readonly IRepository<Postal, long> _postalRepository = postalRepository;
        private readonly IRepository<Bag, int> _bagRepository = bagRepository;
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

        public async Task<ItemDetails> GetItem(string trackingNo)
        {
            var items = await Repository.GetAllListAsync(x => x.Id.Equals(trackingNo));

            if (items.Count == 0) throw new UserFriendlyException("No Item under this Tracking Id is found.");

            Item foundItem = null;
            Bag bag = null;
            Dispatch dispatch = null;

            foreach (var item in items)
            {
                var temp_dispatch = await _dispatchRepository.FirstOrDefaultAsync(x => x.Id.Equals(item.DispatchID));
                if (temp_dispatch is not null)
                {
                    bool isTemp = false;
                    if (temp_dispatch.DispatchNo.Contains("temp", StringComparison.CurrentCultureIgnoreCase)) isTemp = true;

                    foundItem = item;
                    bag = isTemp ? null : await _bagRepository.FirstOrDefaultAsync(x => x.Id.Equals(item.BagID));
                    dispatch = temp_dispatch;
                }
            }

            var postal = await _postalRepository.FirstOrDefaultAsync(x =>
                                                                    x.PostalCode.Equals(dispatch.PostalCode) &&
                                                                    x.ServiceCode.Equals(dispatch.ServiceCode) &&
                                                                    x.ProductCode.Equals(dispatch.ProductCode)
                                                                );
            string ProductDesc = "";
            string ServiceDesc = "";
            if (postal is null)
            {
                var postals = await _postalRepository.GetAllListAsync();
                var postalDistinctedByProductCode = postals.DistinctBy(x => x.ProductCode).ToList();
                var postalDistinctedByServiceCode = postals.DistinctBy(x => x.ServiceCode).ToList();
                ProductDesc = postalDistinctedByProductCode.FirstOrDefault(x => x.ProductCode.Equals(dispatch.ProductCode)).ProductDesc;
                ServiceDesc = postalDistinctedByServiceCode.FirstOrDefault(x => x.ServiceCode.Equals(dispatch.ServiceCode)).ServiceDesc;
            }

            string address = foundItem.Address is null ? "" : foundItem.Address + ", ";
            string city = foundItem.City is null ? "" : foundItem.City + ", ";
            string state = foundItem.State is null ? "" : foundItem.State + ", ";
            string postcode = foundItem.Postcode is null ? "" : foundItem.Postcode + ", ";
            string country = foundItem.CountryCode is null ? "" : foundItem.CountryCode;
            string addressString = string.Format("{0} {1} {2} {3} {4}", address, city, state, postcode, country);
            return new()
            {
                TrackingNo = trackingNo,
                DispatchNo = dispatch.DispatchNo,
                BagNo = bag is null ? "" : bag.BagNo,
                DispatchDate = $"{dispatch.DispatchDate:dd/MM/yyyy}",
                Postal = postal is null ? dispatch.PostalCode : postal.PostalDesc,
                Service = postal is null ? ServiceDesc : postal.ServiceDesc,
                Product = postal is null ? ProductDesc : postal.ProductDesc,
                Country = foundItem.CountryCode,
                Weight = foundItem.Weight is null ? 0.000m : (decimal)foundItem.Weight,
                Status = foundItem.Status is null ? 0 : (int)foundItem.Status,
                Value = foundItem.ItemValue is null ? 0.00m : (decimal)foundItem.ItemValue,
                Description = foundItem.ItemDesc is null ? "" : foundItem.ItemDesc,
                ReferenceNo = foundItem.RefNo is null ? "" : foundItem.RefNo,
                Recipient = foundItem.RecpName is null ? "" : foundItem.RecpName,
                ContactNo = foundItem.TelNo is null ? "" : foundItem.TelNo,
                Email = foundItem.Email is null ? "" : foundItem.Email,
                Address = addressString,
            };
        }

        [HttpPost]
        public async Task<List<APIItemIdByDistinctAndDay>> GetAPIItemIdByDistinctAndDay(GetAPIItemIdDetail input)
        {
            List<APIItemIdByDistinctAndDay> result = [];
            DateTime firstDayOfMonth = DateTime.ParseExact($"{input.Month} {input.Year}", "MMMM yyyy", CultureInfo.InvariantCulture);

            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int daysBetween = (int)(lastDayOfMonth - firstDayOfMonth).TotalDays + 1;

            var itemTrackings = await _itemTrackingRepository.GetAllListAsync(x =>
                                                           !x.IsExternal &&
                                                            x.DateUsed <= lastDayOfMonth &&
                                                            x.DateUsed >= firstDayOfMonth);

            var distinctItemIds = itemTrackings
                                        .Select(x => x.TrackingNo)
                                        .Distinct()
                                        .ToList();

            var distinctDispatchIds = itemTrackings
                                        .Select(x => x.DispatchId)
                                        .Distinct()
                                        .ToList();

            var items = await Repository.GetAllListAsync(x => distinctItemIds.Contains(x.Id));

            var dispatches = await _dispatchRepository.GetAllListAsync(x => distinctDispatchIds.Contains(x.Id));

            var joinedItems = (from itemTracking in itemTrackings
                               join dispatch in dispatches on itemTracking.DispatchId equals dispatch.Id
                               join item in items on itemTracking.TrackingNo equals item.Id
                               select new
                               {
                                   dispatch.CustomerCode,
                                   dispatch.DispatchNo,
                                   item.ProductCode,
                                   dispatch.ServiceCode,
                                   dispatch.PostalCode,
                                   itemTracking.TrackingNo,
                                   item.DispatchDate,
                                   itemTracking.DispatchId,
                                   itemTracking.DateUsed,
                                   item.ItemValue,
                                   item.Weight
                               }).Where(x =>
                                            x.CustomerCode.Equals(input.CustomerCode) &&
                                            x.ProductCode.Equals(input.ProductCode) &&
                                            x.ServiceCode.Equals(input.ServiceCode) &&
                                            x.PostalCode.Equals(input.PostalCode)
                               ).ToList();

            var uploadedItems = joinedItems //done both pre-reg and uploaded
                                    .GroupBy(item => item.TrackingNo)
                                    .Where(group => group.Count() > 1)
                                    .SelectMany(group => group)
                                    .OrderByDescending(x => x.DispatchDate)
                                    .DistinctBy(x => x.TrackingNo)
                                    .ToList();

            var pendingItems = joinedItems //done pre-reg but not uploaded
                                    .GroupBy(item => item.TrackingNo)
                                    .Where(group => group.Count() == 1)
                                    .SelectMany(group => group.Where(x => x.DispatchNo.Contains("Temp")))
                                    .ToList();

            var unregisteredItems = joinedItems //not done pre-reg and uploaded
                                    .GroupBy(item => item.TrackingNo)
                                    .Where(group => group.Count() == 1)
                                    .SelectMany(group => group.Where(x => !x.DispatchNo.Contains("Temp")))
                                    .ToList();


            for (int i = 0; i < daysBetween; i++)
            {
                DateOnly currentDate = DateOnly.FromDateTime(firstDayOfMonth.AddDays(i));

                var filtered_uploadedItems = uploadedItems.Where(x => DateOnly.FromDateTime(x.DateUsed).Equals(currentDate)).ToList();
                var filtered_pendingItems = pendingItems.Where(x => DateOnly.FromDateTime(x.DateUsed).Equals(currentDate)).ToList();
                var filtered_unregisteredItems = unregisteredItems.Where(x => DateOnly.FromDateTime(x.DateUsed).Equals(currentDate)).ToList();

                if (filtered_uploadedItems.Count > 0 || filtered_pendingItems.Count > 0 || filtered_unregisteredItems.Count > 0)
                {
                    result.Add(new APIItemIdByDistinctAndDay()
                    {
                        TotalItems_Uploaded = filtered_uploadedItems.Count,
                        TotalItems_Pending = filtered_pendingItems.Count,
                        TotalItems_Unregistered = filtered_unregisteredItems.Count,
                        TotalWeight_Uploaded = Math.Round((decimal)filtered_uploadedItems.Sum(x => x.Weight), 3),
                        TotalWeight_Pending = Math.Round((decimal)filtered_pendingItems.Sum(x => x.Weight), 3),
                        TotalWeight_Unregistered = Math.Round((decimal)filtered_unregisteredItems.Sum(x => x.Weight), 3),
                        AverageValue_Uploaded = Math.Round((decimal)filtered_uploadedItems.Sum(x => x.ItemValue), 2),
                        AverageValue_Pending = Math.Round((decimal)filtered_pendingItems.Sum(x => x.ItemValue), 2),
                        AverageValue_Unregistered = Math.Round((decimal)filtered_unregisteredItems.Sum(x => x.ItemValue), 2),
                        Date = currentDate.ToString("dd/MM/yyyy")
                    });
                }
            }

            result = [.. result.OrderByDescending(x => x.Date)];

            return result;
        }

        public async Task<PagedResultDto<APIItemIdDashboard>> GetAPIItemIdDashboard(string month, string year, int MaxResultCount, int SkipCount)
        {
            List<APIItemIdDashboard> apiItemIdDashboard = [];

            DateOnly firstDayOfMonth = DateOnly.FromDateTime(DateTime.ParseExact($"{month} {year}", "MMMM yyyy", CultureInfo.InvariantCulture));

            DateOnly lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var postals = await _postalRepository.GetAllListAsync();
            var postalDistinctedByProductCode = postals.DistinctBy(x => x.ProductCode).ToList();
            var postalDistinctedByServiceCode = postals.DistinctBy(x => x.ServiceCode).ToList();

            var itemTrackings = await _itemTrackingRepository.GetAllListAsync(x =>
                                        !x.IsExternal &&
                                        DateOnly.FromDateTime(x.DateUsed) <= lastDayOfMonth &&
                                        DateOnly.FromDateTime(x.DateUsed) >= firstDayOfMonth);

            var distinctDispatchIds = itemTrackings
                                        .Select(x => x.DispatchId)
                                        .Distinct()
                                        .ToList();

            var dispatches = await _dispatchRepository.GetAllListAsync(x => distinctDispatchIds.Contains(x.Id));

            var distinctedCombination = (from itemTracking in itemTrackings
                                         join dispatch in dispatches on itemTracking.DispatchId equals dispatch.Id
                                         select new
                                         {
                                             dispatch.CustomerCode,
                                             itemTracking.ProductCode,
                                             dispatch.ServiceCode,
                                             dispatch.PostalCode
                                         }).Distinct().ToList();

            int totalCount = distinctedCombination.Count;

            distinctedCombination = distinctedCombination.Skip(SkipCount).Take(MaxResultCount).ToList();

            var joinedItems = (from itemTracking in itemTrackings
                               join dispatch in dispatches on itemTracking.DispatchId equals dispatch.Id
                               select new
                               {
                                   dispatch.CustomerCode,
                                   itemTracking.ProductCode,
                                   dispatch.ServiceCode,
                                   dispatch.PostalCode,
                                   itemTracking.TrackingNo,
                                   dispatch.DispatchDate,
                                   itemTracking.DateUsed
                               }).OrderByDescending(x => x.DateUsed)
                                 .DistinctBy(x => x.TrackingNo)
                                 .ToList();

            foreach (var distincted in distinctedCombination)
            {
                var filteredItems = joinedItems.Where(x =>
                                                        x.CustomerCode.Trim().Equals(distincted.CustomerCode.Trim()) &&
                                                        x.ProductCode .Trim().Equals(distincted.ProductCode .Trim()) &&
                                                        x.ServiceCode .Trim().Equals(distincted.ServiceCode .Trim()) &&
                                                        x.PostalCode  .Trim().Equals(distincted.PostalCode  .Trim())
                                                     ).OrderByDescending(x => x.DateUsed).ToList();

                var postal = postals.FirstOrDefault(x => x.PostalCode .Trim().Equals(distincted.PostalCode .Trim()) &&
                                                         x.ProductCode.Trim().Equals(distincted.ProductCode.Trim()) &&
                                                         x.ServiceCode.Trim().Equals(distincted.ServiceCode.Trim()));

                
                string productDesc = postal is null ? postalDistinctedByProductCode.FirstOrDefault(x => x.ProductCode.Equals(distincted.ProductCode)).ProductDesc : postal.ProductDesc;
                string serviceDesc = postal is null ? postalDistinctedByServiceCode.FirstOrDefault(x => x.ServiceCode.Equals(distincted.ServiceCode)).ServiceDesc : postal.ServiceDesc;

                apiItemIdDashboard.Add(new APIItemIdDashboard()
                {
                    CustomerCode = distincted.CustomerCode,
                    PostalCode = postal is null ? distincted.PostalCode : postal.PostalCode,
                    ServiceCode = postal is null ? distincted.ServiceCode : postal.ServiceCode,
                    ProductCode = postal is null ? distincted.ProductCode : postal.ProductCode,
                    PostalDesc = postal is null ? distincted.PostalCode : postal.PostalDesc,
                    ServiceDesc = serviceDesc.Trim(),
                    ProductDesc = productDesc.Trim(),
                    TotalItems = filteredItems.Count,
                    DateLastReceived = filteredItems[0].DateUsed.ToString()
                });

                apiItemIdDashboard = [.. apiItemIdDashboard.OrderByDescending(x => x.DateLastReceived)];
            }

            return new PagedResultDto<APIItemIdDashboard>(
                totalCount,
                apiItemIdDashboard
            );
        }
    }
}
