using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.RateWeightBreaks.Dto;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SND.SMP.PostalOrgs;
using SND.SMP.Postals;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SND.SMP.Currencies;
using SND.SMP.Rates;
using System.Configuration.Internal;
using Microsoft.EntityFrameworkCore;

namespace SND.SMP.RateWeightBreaks
{
    public class RateWeightBreakAppService : AsyncCrudAppService<RateWeightBreak, RateWeightBreakDto, int, PagedRateWeightBreakResultRequestDto>
    {
        private readonly IRepository<Rate, int> _rateRepository;
        private readonly IRepository<Currency, long> _currencyRepository;
        private readonly IRepository<PostalOrg, string> _postalOrgRepository;

        public RateWeightBreakAppService(
            IRepository<RateWeightBreak, int> repository,
            IRepository<Rate, int> rateRepository,
            IRepository<Currency, long> currencyRepository,
            IRepository<PostalOrg, string> postalOrgRepository
        ) : base(repository)
        {
            _rateRepository = rateRepository;
            _currencyRepository = currencyRepository;
            _postalOrgRepository = postalOrgRepository;
        }
        protected override IQueryable<RateWeightBreak> CreateFilteredQuery(PagedRateWeightBreakResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.PostalOrgId.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword)).AsQueryable();
        }

        #region Private Functions
        private async Task<long> InsertAndGetIdForCurrency(string currency)
        {
            var currencyCreateId = await _currencyRepository.InsertAsync(new Currency()
            {
                Abbr = currency,
                Description = ""
            });
            await _currencyRepository.GetDbContext().SaveChangesAsync();
            return currencyCreateId.Id;
        }

        private async Task<string> InsertAndGetIdForPostalOrg(string postal)
        {
            var existingPostalOrg = await _postalOrgRepository.FirstOrDefaultAsync(x => x.Id.Equals(postal));

            if (existingPostalOrg != null)
            {
                existingPostalOrg.Name = "";
                await _postalOrgRepository.UpdateAsync(existingPostalOrg);
                await _postalOrgRepository.GetDbContext().SaveChangesAsync();

                return existingPostalOrg.Id;
            }
            else
            {
                var newPostalOrg = new PostalOrg()
                {
                    Id = postal,
                    Name = ""
                };

                await _postalOrgRepository.InsertAsync(newPostalOrg);

                await _postalOrgRepository.GetDbContext().SaveChangesAsync();

                return newPostalOrg.Id;
            }
        }


        private async Task<List<Rate>> InsertAndGetUpdatedRateCards(List<string> rateCards)
        {
            List<Rate> rateCard = [];

            foreach (var distinctedRateCard in rateCards)
            {
                var rate = await _rateRepository.FirstOrDefaultAsync(x => x.CardName.Equals(distinctedRateCard));

                rateCard.Add(new Rate()
                {
                    CardName = distinctedRateCard,
                    Id = rate is null ? 0 : rate.Id,
                    Count = 1
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
                    rc.Id = rateCardCreateId;
                }
                else
                {
                    var rateCardForUpdate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rc.Id));
                    rateCardForUpdate.Count += rc.Count;
                    var rateCardUpdate = await _rateRepository.UpdateAsync(rateCardForUpdate);
                }
            }

            return rateCard;
        }
        #endregion

        public async Task<RateCardWeightBreakDisplayDto> GetRateWeightBreakByRate(int rateid)
        {
            var rateWeightBreaks = await Repository.GetAllListAsync(x => x.RateId.Equals(rateid));

            var rateWeight = rateWeightBreaks.FirstOrDefault();

            var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.RateId));
            var postalOrg = await _postalOrgRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.PostalOrgId));
            var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.CurrencyId));

            List<WeightBreakDisplayDto> wb = [];
            foreach (RateWeightBreak rwb in rateWeightBreaks)
            {
                wb.Add(new WeightBreakDisplayDto()
                {
                    WeightBreak = rwb.WeightMin.ToString() + " - " + rwb.WeightMax.ToString(),
                    ProductCode = rwb.ProductCode,
                    ItemRate = rwb.ItemRate,
                    WeightRate = rwb.WeightRate,
                    IsExceedRule = rwb.IsExceedRule,
                });
            }

            return new RateCardWeightBreakDisplayDto()
            {
                RateCardName = rate.CardName,
                Currency = currency.Abbr,
                Postal = postalOrg.Id,
                PaymentMode = rateWeight.PaymentMode,
                WeightBreaks = wb
            };
        }

        [Consumes("multipart/form-data")]
        public async Task<List<RateCardWeightBreakDto>> UploadRateWeightBreakFile([FromForm] UploadPostal input)
        {
            try
            {
                if (input.file == null || input.file.Length == 0) return [];

                const string EXCEEDS = "Exceeds";

                DataSet dataSet = new();

                List<RateCardWeightBreakDto> listRateCards = [];
                List<string> rateCards = [];
                List<Rate> rateCard = [];
                List<RateWeightBreak> insert = [];

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var postalOrgs = await _postalOrgRepository.GetAllListAsync();
                var currencies = await _currencyRepository.GetAllListAsync();

                using (var package = new ExcelPackage(input.file.OpenReadStream()))
                {
                    var worksheets = package.Workbook.Worksheets;

                    foreach (ExcelWorksheet ws in worksheets)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable = ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].ToDataTable(c =>
                        {
                            c.FirstRowIsColumnNames = false;
                        });
                        dataTable.TableName = ws.Name;
                        dataSet.Tables.Add(dataTable);
                        rateCards.Add(ws.Name);
                    }

                    var tables = dataSet.Tables;

                    rateCard = await InsertAndGetUpdatedRateCards(rateCards);

                    foreach (DataTable table in tables)
                    {
                        List<WeightBreakDto> listWeightBreak = [];

                        var rateCardName = Convert.ToString(table.Rows[0][1]);
                        var currency = Convert.ToString(table.Rows[0][4]);
                        var postal = Convert.ToString(table.Rows[0][7]);
                        var paymentMode = Convert.ToString(table.Rows[0][10]);

                        var productCodes = table.Rows[1].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(u => Convert.ToString(u)).ToList();

                        var rate = rateCard.FirstOrDefault(x => x.CardName.Equals(rateCardName));
                        var curr = currencies.FirstOrDefault(x => x.Abbr.Equals(currency));
                        var postalorg = postalOrgs.FirstOrDefault(x => x.Id.Equals(postal));

                        long currId = curr is not null ? curr.Id : await InsertAndGetIdForCurrency(currency);
                        string postalOrgId = postalorg is not null ? postalorg.Id : await InsertAndGetIdForPostalOrg(postal);

                        for (int i = 3; i < table.Rows.Count; i++)
                        {
                            int colIndex = 2;
                            foreach (var productCode in productCodes)
                            {
                                bool IsExceedRule = Convert.ToString(table.Rows[i][0]) == EXCEEDS;

                                var wb = new WeightBreakDto()
                                {
                                    IsExceedRule = IsExceedRule,
                                    WeightMinKg = !IsExceedRule ? Convert.ToDecimal(table.Rows[i][0] ?? 0) / 1000 : 0,
                                    WeightMaxKg = !IsExceedRule ? Convert.ToDecimal(table.Rows[i][1] ?? 0) / 1000 : 0,
                                    ProductCode = productCode!,
                                    ItemRate = table.Rows[i][colIndex] == DBNull.Value ? 0 : Convert.ToDecimal(table.Rows[i][colIndex]),
                                    WeightRate = table.Rows[i][colIndex + 1] == DBNull.Value ? 0 : Convert.ToDecimal(table.Rows[i][colIndex + 1]),
                                };

                                // if(!wb.ItemRate.Equals(Convert.ToDecimal(0)) && !wb.WeightRate.Equals(Convert.ToDecimal(0)))
                                // {
                                listWeightBreak.Add(wb);

                                insert.Add(new RateWeightBreak()
                                {
                                    RateId = rate.Id,
                                    PostalOrgId = postalOrgId,
                                    WeightMin = wb.WeightMinKg,
                                    WeightMax = wb.WeightMaxKg,
                                    ProductCode = wb.ProductCode,
                                    CurrencyId = currId,
                                    ItemRate = wb.ItemRate,
                                    WeightRate = wb.WeightRate,
                                    IsExceedRule = wb.IsExceedRule,
                                    PaymentMode = paymentMode
                                });
                                // }


                                colIndex += 2;
                            }
                        }

                        listRateCards.Add(new RateCardWeightBreakDto
                        {
                            RateCardName = rateCardName!,
                            Currency = currency!,
                            Postal = postal!,
                            PaymentMode = paymentMode!,
                            WeightBreaks = listWeightBreak
                        });
                    }
                    //Insert into DB

                    //----Delete Existing Rate Cards in RateWeightBreak ----//
                    foreach (var distinctedRateCard in rateCard)
                    {
                        var rwb = await Repository.GetAllListAsync(x => x.RateId.Equals(distinctedRateCard.Id));
                        if (rwb.Count > 0) await Repository.GetDbContext().Database.ExecuteSqlAsync($"DELETE FROM smpdb.rateweightbreaks WHERE RateId = '{distinctedRateCard.Id.ToString()}'");
                    }

                    foreach (RateWeightBreak rwb in insert)
                    {
                        var insertedItem = await Repository.InsertAsync(rwb);
                        await Repository.GetDbContext().SaveChangesAsync();
                    }

                }

                return listRateCards;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return new List<RateCardWeightBreakDto>();
            }

        }
    }
}
