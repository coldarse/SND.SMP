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
using SND.SMP.RateItems.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using SND.SMP.Rates;
using SND.SMP.Currencies;
using OfficeOpenXml;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Abp.Application.Services.Dto;
using System.Linq.Dynamic.Core;

namespace SND.SMP.RateItems
{
    public class RateItemAppService(
        IRepository<RateItem, long> repository,
        IRepository<Rate, int> rateRepository,
        IRepository<Currency, long> currencyRepository
        ) : AsyncCrudAppService<RateItem, RateItemDto, long, PagedRateItemResultRequestDto>(repository)
    {
        protected override IQueryable<RateItem> CreateFilteredQuery(PagedRateItemResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.RateId.ToString().Equals(input.Keyword));
        }

        private IQueryable<RateItemDetailDto> ApplySorting(IQueryable<RateItemDetailDto> query, PagedRateItemResultRequestDto input)
        {
            //Try to sort query if available
            if (input is ISortedResultRequest sortInput)
            {
                if (!sortInput.Sorting.IsNullOrWhiteSpace())
                {
                    return query.OrderBy(sortInput.Sorting);
                }
            }

            //IQueryable.Task requires sorting, so we should sort if Take will be used.
            if (input is ILimitedResultRequest)
            {
                return query.OrderByDescending(e => e.Id);
            }

            //No sorting
            return query;
        }

        private IQueryable<RateItemDetailDto> ApplyPaging(IQueryable<RateItemDetailDto> query, PagedRateItemResultRequestDto input)
        {
            if ((object)input is IPagedResultRequest pagedResultRequest)
            {
                return query.PageBy(pagedResultRequest);
            }

            if ((object)input is ILimitedResultRequest limitedResultRequest)
            {
                return query.Take(limitedResultRequest.MaxResultCount);
            }

            return query;
        }

        public async Task<int> GetAllRateItemsCount()
        {
            return await Repository.CountAsync();
        }

        public async Task<FullRateItemDetailDto> GetFullRateItemDetail(PagedRateItemResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = CreateFilteredQuery(input);

            var rates = await rateRepository.GetAllListAsync();

            var currencies = await currencyRepository.GetAllListAsync();

            List<RateItemDetailDto> detailed = [];

            foreach (var rateItem in query.ToList())
            {
                var rate = rates.FirstOrDefault(x => x.Id.Equals(rateItem.RateId));

                var currency = currencies.FirstOrDefault(x => x.Id.Equals(rateItem.CurrencyId));

                detailed.Add(new RateItemDetailDto()
                {
                    Id = rateItem.Id,
                    RateId = rateItem.RateId,
                    RateCardName = rate.CardName,
                    ServiceCode = rateItem.ServiceCode,
                    ProductCode = rateItem.ProductCode,
                    CountryCode = rateItem.CountryCode,
                    Total = rateItem.Total,
                    Fee = rateItem.Fee,
                    CurrencyId = rateItem.CurrencyId,
                    Currency = currency.Abbr,
                    PaymentMode = rateItem.PaymentMode,
                });
            }

            var totalCount = detailed.Count;

            detailed = [.. ApplySorting(detailed.AsQueryable(), input)];
            detailed = [.. ApplyPaging(detailed.AsQueryable(), input)];

            int rateItemCount = await Repository.CountAsync();

            rates.Insert(0, new Rate()
            {
                Id = 0,
                CardName = "All",
                Count = rateItemCount
            });

            return new FullRateItemDetailDto()
            {
                PagedRateItemResultDto = new PagedResultDto<RateItemDetailDto>(totalCount, [..detailed]),
                Rates = rates
            };
        }

        private async Task<DataTable> ConvertToDatatable(Stream ms)
        {
            DataTable dataTable = new();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(ms))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                // Assuming the first row is the header
                for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                {
                    string columnName = worksheet.Cells[1, i].Value?.ToString();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        dataTable.Columns.Add(columnName);
                    }
                }

                // Populate DataTable with data from Excel
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

        [Consumes("multipart/form-data")]
        public async Task<List<RateItem>> UploadRateItemFile([FromForm] UploadRateItem input)
        {
            if (input.file == null || input.file.Length == 0) return [];

            DataTable dataTable = await ConvertToDatatable(input.file.OpenReadStream());

            List<RateItemExcel> rateItemExcel = [];
            foreach (DataRow dr in dataTable.Rows)
            {
                rateItemExcel.Add(new RateItemExcel()
                {
                    RateCard = dr.ItemArray[0].ToString(),
                    ServiceCode = dr.ItemArray[1].ToString(),
                    ProductCode = dr.ItemArray[2].ToString(),
                    CountryCode = dr.ItemArray[3].ToString(),
                    Total = dr.ItemArray[4].ToString() == "" ? 0 : Convert.ToDecimal(dr.ItemArray[4]),
                    Fee = dr.ItemArray[5].ToString() == "" ? 0 : Convert.ToDecimal(dr.ItemArray[5]),
                    Currency = dr.ItemArray[6].ToString(),
                    PaymentMode = dr.ItemArray[7].ToString()
                });
            }

            var distinctedByRateCards = rateItemExcel.DistinctBy(x => x.RateCard);

            List<Rate> rateCard = [];
            foreach (var distinctedRateCard in distinctedByRateCards)
            {
                int distinctedCount = rateItemExcel.Count(x => x.RateCard.Equals(distinctedRateCard.RateCard));

                var rate = await rateRepository.FirstOrDefaultAsync(x => x.CardName.Equals(distinctedRateCard.RateCard));

                rateCard.Add(new Rate()
                {
                    CardName = distinctedRateCard.RateCard,
                    Id = rate is null ? 0 : rate.Id,
                    Count = distinctedCount
                });
            }

            var distinctedByCurrency = rateItemExcel.DistinctBy(x => x.Currency);

            List<Currency> currency = [];
            foreach (var distinctedCurrency in distinctedByCurrency)
            {
                var curr = await currencyRepository.FirstOrDefaultAsync(x => x.Abbr.Equals(distinctedCurrency.Currency));

                currency.Add(new Currency()
                {
                    Abbr = distinctedCurrency.Currency,
                    Description = curr is null ? "" : curr.Description,
                    Id = curr is null ? 0 : curr.Id
                });
            }

            foreach (var rc in rateCard)
            {
                if (rc.Id.Equals(0))
                {
                    var rateCardCreateId = await rateRepository.InsertAndGetIdAsync(new Rate()
                    {
                        CardName = rc.CardName,
                        Count = rc.Count
                    });

                    var updatedRateCard = rateCard.FirstOrDefault(x => x.CardName.Equals(rc.CardName));
                    updatedRateCard.Id = rateCardCreateId;
                }
                else
                {
                    var rateCardForUpdate = await rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rc.Id));
                    rateCardForUpdate.Count = rc.Count;
                    var rateCardUpdate = await rateRepository.UpdateAsync(rateCardForUpdate);
                }
            }

            foreach (var c in currency.Where(x => x.Id.Equals(0)))
            {
                var currencyCreateId = await currencyRepository.InsertAndGetIdAsync(new Currency()
                {
                    Abbr = c.Abbr,
                    Description = ""
                });

                var updatedCurrency = currency.FirstOrDefault(x => x.Abbr.Equals(c.Abbr));
                updatedCurrency.Id = currencyCreateId;
            }

            await Repository.GetDbContext().Database.ExecuteSqlRawAsync("TRUNCATE TABLE smpdb.rateitems");

            List<RateItem> rateItems = [];
            foreach (RateItemExcel excelItem in rateItemExcel.ToList())
            {
                int rateCardForInsert = rateCard.FirstOrDefault(x => x.CardName.Equals(excelItem.RateCard)).Id;
                long currencyForInsert = currency.FirstOrDefault(x => x.Abbr.Equals(excelItem.Currency)).Id;

                var insert = await Repository.InsertAsync(new RateItem()
                {
                    RateId = rateCardForInsert,
                    ServiceCode = excelItem.ServiceCode,
                    ProductCode = excelItem.ProductCode,
                    CountryCode = excelItem.CountryCode,
                    Total = excelItem.Total,
                    Fee = excelItem.Fee,
                    CurrencyId = currencyForInsert,
                    PaymentMode = excelItem.PaymentMode
                });

                rateItems.Add(insert);
            }
            return rateItems;
        }
    }
}
