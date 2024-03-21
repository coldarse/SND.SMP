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

namespace SND.SMP.RateWeightBreaks
{
    public class RateWeightBreakAppService : AsyncCrudAppService<RateWeightBreak, RateWeightBreakDto, int, PagedRateWeightBreakResultRequestDto>
    {

        public RateWeightBreakAppService(IRepository<RateWeightBreak, int> repository) : base(repository)
        {
        }
        protected override IQueryable<RateWeightBreak> CreateFilteredQuery(PagedRateWeightBreakResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.PostalOrgId.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword)).AsQueryable();
        }

        [Consumes("multipart/form-data")]
        public async Task<List<RateCardWeightBreakDto>> UploadRateWeightBreakFile([FromForm] UploadPostal input)
        {
            try
            {
                if (input.file == null || input.file.Length == 0) return new List<RateCardWeightBreakDto>();

                DataSet dataSet = new DataSet();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var listRateCards = new List<RateCardWeightBreakDto>();

                const string EXCEEDS = "Exceeds";

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
                    }

                    var tables = dataSet.Tables;

                    foreach (DataTable table in tables)
                    {
                        var rateCardName = Convert.ToString(table.Rows[0][1]);
                        var currency = Convert.ToString(table.Rows[0][4]);
                        var postal = Convert.ToString(table.Rows[0][7]);
                        var paymentMode = Convert.ToString(table.Rows[0][10]);

                        var productCodes = table.Rows[1].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(u => Convert.ToString(u)).ToList();

                        var listWeightBreak = new List<WeightBreakDto>();

                        for (int i = 3; i < table.Rows.Count; i++)
                        {
                            int colIndex = 2;
                            foreach (var productCode in productCodes)
                            {
                                var wb = new WeightBreakDto();
                                wb.IsExceedRule = Convert.ToString(table.Rows[i][0]) == EXCEEDS ? true : false;
                                wb.WeightMinKg = !wb.IsExceedRule ? Convert.ToDecimal(table.Rows[i][0] ?? 0) / 1000 : null;
                                wb.WeightMaxKg = !wb.IsExceedRule ? Convert.ToDecimal(table.Rows[i][1] ?? 0) / 1000 : null;
                                wb.ProductCode = productCode!;
                                wb.ItemRate = table.Rows[i][colIndex] == DBNull.Value ? null : Convert.ToDecimal(table.Rows[i][colIndex]);
                                wb.WeightRate = table.Rows[i][colIndex + 1] == DBNull.Value ? null : Convert.ToDecimal(table.Rows[i][colIndex + 1]);

                                listWeightBreak.Add(wb);

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
