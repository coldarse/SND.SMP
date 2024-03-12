using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDataReader;
using SND.SMP.RateWeightBreak.Dto;

namespace SND.SMP.RateWeightBreak
{
    public class RateWeightBreakUtil
    {
        public RateWeightBreakUtil()
        {
        }

        public List<RateCardWeightBreakDto> ReadRateWeightBreak(string filePath)
        {
            var listRateCards = new List<RateCardWeightBreakDto>();
            const string EXCEEDS = "Exceeds";

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();

                    var tables = result.Tables;

                    foreach (System.Data.DataTable table in tables)
                    {
                        var rateCardName = Convert.ToString(table.Rows[0][1]);
                        var currency = Convert.ToString(table.Rows[0][4]);
                        var postal = Convert.ToString(table.Rows[0][7]);
                        var paymentMode = Convert.ToString(table.Rows[0][10]);

                        var productCodes = table.Rows[2].ItemArray.Where(u => !string.IsNullOrWhiteSpace(Convert.ToString(u))).Select(u => Convert.ToString(u)).ToList();

                        var listWeightBreak = new List<WeightBreakDto>();

                        for (int i = 4; i < table.Rows.Count; i++)
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
            }

            return listRateCards;
        }
    }
}

