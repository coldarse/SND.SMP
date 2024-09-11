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
using Abp.Linq.Extensions;
using System.Text.Json;

namespace SND.SMP.RateWeightBreaks
{
    public class RateWeightBreakAppService(
        IRepository<RateWeightBreak, int> repository,
        IRepository<Rate, int> rateRepository,
        IRepository<Currency, long> currencyRepository,
        IRepository<PostalOrg, string> postalOrgRepository
        ) : AsyncCrudAppService<RateWeightBreak, RateWeightBreakDto, int, PagedRateWeightBreakResultRequestDto>(repository)
    {
        private readonly IRepository<Rate, int> _rateRepository = rateRepository;
        private readonly IRepository<Currency, long> _currencyRepository = currencyRepository;
        private readonly IRepository<PostalOrg, string> _postalOrgRepository = postalOrgRepository;

        protected override IQueryable<RateWeightBreak> CreateFilteredQuery(PagedRateWeightBreakResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.PostalOrgId.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword));
        }

        private async Task<long> InsertAndGetIdForCurrency(string currency)
        {
            var currencyCreateId = await _currencyRepository.InsertAsync(new Currency()
            {
                Abbr = currency,
                Description = ""
            });
            await _currencyRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);
            return currencyCreateId.Id;
        }
        private async Task<string> InsertAndGetIdForPostalOrg(string postal)
        {
            var existingPostalOrg = await _postalOrgRepository.FirstOrDefaultAsync(x => x.Id.Equals(postal));

            if (existingPostalOrg != null)
            {
                existingPostalOrg.Name = "";
                await _postalOrgRepository.UpdateAsync(existingPostalOrg);
                await _postalOrgRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

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

                await _postalOrgRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

                return newPostalOrg.Id;
            }
        }
        private async Task<List<Rate>> InsertAndGetUpdatedRateCards(List<string> rateCards)
        {
            List<Rate> rateCard = [];

            // var de_rates = await _rateRepository.GetAllListAsync(x => x.Service.Equals("DE"));
            // foreach (var rate in de_rates)
            // {
            //     rate.Count = 0;
            //     rate.Service = "";
            //     await _rateRepository.UpdateAsync(rate);
            // }
            await _rateRepository.GetDbContext().SaveChangesAsync().ConfigureAwait(false);

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
                        Count = rc.Count,
                        Service = "DE"
                    });

                    var updatedRateCard = rateCard.FirstOrDefault(x => x.CardName.Equals(rc.CardName));
                    updatedRateCard.Id = rateCardCreateId;
                    rc.Id = rateCardCreateId;
                }
                else
                {
                    var rateCardForUpdate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rc.Id));
                    rateCardForUpdate.Count = rc.Count;
                    rateCardForUpdate.Service = "DE";
                    var rateCardUpdate = await _rateRepository.UpdateAsync(rateCardForUpdate);
                }
            }

            return rateCard;
        }


        public async Task<RateCardWeightBreakDisplayDto> GetRateWeightBreakByRate(int rateid)
        {
            var rateWeightBreaks = await Repository.GetAllListAsync(x => x.RateId.Equals(rateid));

            var rateWeight = rateWeightBreaks.FirstOrDefault();

            var rate = await _rateRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.RateId));
            var postalOrg = await _postalOrgRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.PostalOrgId));
            var currency = await _currencyRepository.FirstOrDefaultAsync(x => x.Id.Equals(rateWeight.CurrencyId));

            List<WeightBreakDisplayDto> wbs = [];
            foreach (RateWeightBreak rwb in rateWeightBreaks)
            {
                wbs.Add(new WeightBreakDisplayDto()
                {
                    WeightBreak = rwb.WeightMin.ToString() + " - " + rwb.WeightMax.ToString(),
                    ProductCode = rwb.ProductCode,
                    Zone = rwb.Zone,
                    ItemRate = rwb.ItemRate,
                    WeightRate = rwb.WeightRate,
                    IsExceedRule = rwb.IsExceedRule,
                });
            }

            var distinctedProduct = wbs.DistinctBy(x => (x.ProductCode, x.Zone));
            List<string> products = [];
            foreach (var dp in distinctedProduct.ToList())
            {
                string product = dp.Zone == "" ? dp.ProductCode : dp.ProductCode + " - " + dp.Zone;
                products.Add(product);
            }

            var exceeds = wbs.Where(x => x.WeightBreak.Equals("0.00 - 0.00")).ToList();
            var noExceeds = wbs.Where(x => !x.WeightBreak.Equals("0.00 - 0.00")).ToList();

            var groupedWeightBreaks = noExceeds.GroupBy(dto => dto.WeightBreak).ToDictionary(group => group.Key, group => group.ToList());
            groupedWeightBreaks["Exceeds"] = exceeds;

            return new RateCardWeightBreakDisplayDto()
            {
                RateCardName = rate.CardName,
                Currency = currency.Abbr,
                Postal = postalOrg.Id,
                PaymentMode = rateWeight.PaymentMode,
                WeightBreaks = JsonSerializer.Serialize(groupedWeightBreaks),
                Products = products,
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

                List<string> rateCards = [];
                List<RateCardWeightBreakDto> listRateCards = [];
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
                        DataTable dataTable = new();
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

                        var productCodes = table.Rows[1].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(Convert.ToString).ToList();

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
                        if (rwb.Count > 0) await Repository.GetDbContext().Database.ExecuteSqlAsync($"DELETE FROM tfsdb.rateweightbreaks WHERE RateId = '{distinctedRateCard.Id.ToString()}'");
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
                return [];
            }

        }

        private static List<List<string>> GroupZones(List<string> zones)
        {
            List<List<string>> result = [];
            List<string> currentGroup = [];

            foreach (string zone in zones)
            {
                // If we encounter 'Zone 1' and there is already a current group, start a new group
                if (zone == "Zone 1" && currentGroup.Count > 0)
                {
                    result.Add(new List<string>(currentGroup));
                    currentGroup.Clear();
                }

                currentGroup.Add(zone);
            }

            // Add the last group if it's not empty
            if (currentGroup.Count > 0)
            {
                result.Add(currentGroup);
            }

            return result;
        }

        [Consumes("multipart/form-data")]
        public async Task<List<RateCardWeightBreakDto>> UploadRateWeightBreakFileWithZones([FromForm] UploadPostal input)
        {
            try
            {
                if (input.file == null || input.file.Length == 0) return [];

                const string EXCEEDS = "Exceeds";

                DataSet dataSet = new();

                List<string> rateCards = [];
                List<RateCardWeightBreakDto> listRateCards = [];
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
                        DataTable dataTable = new();
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

                        var rateCardName = table.TableName; //Convert.ToString(table.Rows[0][1]);
                        var currency = Convert.ToString(table.Rows[0][4]);
                        var postal = Convert.ToString(table.Rows[0][7]);
                        var paymentMode = Convert.ToString(table.Rows[0][10]);


                        var zones = table.Rows[1].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(Convert.ToString).ToList();
                        bool containsZone = zones.Any(s => s.Contains("Zone")); // Check if this rate has zones
                        var product_row = containsZone ? 2 : 1; // If rate has zones then set the product code row to +1 row
                        var starting_row = containsZone ? 4 : 3;// If rate has zones then set the starting row to +1 row
                        var grouped_zones = containsZone ? GroupZones(zones) : []; // If rate has zones then group the zones
                        var productCodes = table.Rows[product_row].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(Convert.ToString).ToList();

                        var rate = rateCard.FirstOrDefault(x => x.CardName.Equals(rateCardName));
                        var curr = currencies.FirstOrDefault(x => x.Abbr.Equals(currency));
                        var postalorg = postalOrgs.FirstOrDefault(x => x.Id.Equals(postal));

                        long currId = curr is not null ? curr.Id : await InsertAndGetIdForCurrency(currency);
                        string postalOrgId = postalorg is not null ? postalorg.Id : await InsertAndGetIdForPostalOrg(postal);

                        for (int i = starting_row; i < table.Rows.Count; i++)
                        {
                            int colIndex = 2;
                            foreach (var productCode in productCodes)
                            {
                                if (grouped_zones.Count > 0)
                                {
                                    foreach (var zone in zones)
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
                                            PaymentMode = paymentMode,
                                            Zone = zone
                                        });
                                        colIndex += 2;
                                    }
                                }
                                else
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
                                        PaymentMode = paymentMode,
                                        Zone = ""
                                    });
                                    colIndex += 2;
                                }
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
                return [];
            }

        }
    }
}
