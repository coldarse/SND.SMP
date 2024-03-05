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
using Microsoft.AspNetCore.Http;
using System.Data;
using SND.SMP.Rates;
using SND.SMP.Currencies;
using OfficeOpenXml;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SND.SMP.RateItems
{
    public class RateItemAppService : AsyncCrudAppService<RateItem, RateItemDto, long, PagedRateItemResultRequestDto>
    {

        private readonly IRepository<Rate, int> _rateRepository;
        private readonly IRepository<Currency, long> _currencyRepository;

        public RateItemAppService(
            IRepository<RateItem, long> repository,
        IRepository<Rate, int> rateRepository,
        IRepository<Currency, long> currencyRepository
        ) : base(repository)
        {
            _rateRepository = rateRepository;
            _currencyRepository = currencyRepository;
        }
        protected override IQueryable<RateItem> CreateFilteredQuery(PagedRateItemResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword));
        }

        [Consumes("multipart/form-data")]
        public async Task<List<RateItem>> UploadRateItemFile([FromForm] UploadRateItem input)
        {
            if (input.file == null || input.file.Length == 0) return new List<RateItem>();

            DataTable dataTable = new DataTable();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(input.file.OpenReadStream()))
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

            List<RateItemExcel> rateItemExcel = new List<RateItemExcel>();
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

            List<Rate> rateCard = new List<Rate>();
            foreach (var distinctedRateCard in distinctedByRateCards)
            {
                int distinctedCount = rateItemExcel.Count(x => x.RateCard.Equals(distinctedRateCard.RateCard));

                var rate = await _rateRepository.FirstOrDefaultAsync(x => x.CardName.Equals(distinctedRateCard.RateCard));

                rateCard.Add(new Rate()
                {
                    CardName = distinctedRateCard.RateCard,
                    Id = rate is null ? 0 : rate.Id,
                    Count = distinctedCount
                });
            }

            var distinctedByCurrency = rateItemExcel.DistinctBy(x => x.Currency);

            List<Currency> currency = new List<Currency>();
            foreach (var distinctedCurrency in distinctedByCurrency)
            {
                var curr = await _currencyRepository.FirstOrDefaultAsync(x => x.Abbr.Equals(distinctedCurrency.Currency));

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
                    var rateCardCreateId = await _rateRepository.InsertAndGetIdAsync(new Rate()
                    {
                        CardName = rc.CardName,
                        Count = rc.Count
                    });

                    var updatedRateCard = rateCard.FirstOrDefault(x => x.CardName.Equals(rc.CardName));
                    updatedRateCard.Id = rateCardCreateId;
                }
                else
                {
                    var rateCardForUpdate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rc.Id));
                    rateCardForUpdate.Count = rc.Count;
                    var rateCardUpdate = await _rateRepository.UpdateAsync(rateCardForUpdate);
                }
            }

            foreach (var c in currency)
            {
                if (c.Id.Equals(0))
                {
                    var currencyCreateId = await _currencyRepository.InsertAndGetIdAsync(new Currency()
                    {
                        Abbr = c.Abbr,
                        Description = ""
                    });

                    var updatedCurrency = currency.FirstOrDefault(x => x.Abbr.Equals(c.Abbr));
                    updatedCurrency.Id = currencyCreateId;
                }
            }

            await Repository.GetDbContext().Database.ExecuteSqlRawAsync("TRUNCATE TABLE smpdb.rateitems");

            List<RateItem> rateItems = new List<RateItem>();
            foreach (RateItemExcel excelItem in rateItemExcel)
            {
                int rateCardForInsert = rateCard.FirstOrDefault(x => x.CardName.Equals(excelItem.RateCard)).Id;
                long currencyForInsert = currency.FirstOrDefault(x => x.Abbr.Equals(excelItem.Currency)).Id;

                RateItem insertRateItem = new RateItem()
                {
                    RateId = rateCardForInsert,
                    ServiceCode = excelItem.ServiceCode,
                    ProductCode = excelItem.ProductCode,
                    CountryCode = excelItem.CountryCode,
                    Total = excelItem.Total,
                    Fee = excelItem.Fee,
                    CurrencyId = currencyForInsert,
                    PaymentMode = excelItem.PaymentMode
                };

                var insert = await Repository.InsertAsync(insertRateItem);

                rateItems.Add(insert);
            }

            return rateItems;

        }
    }
}
