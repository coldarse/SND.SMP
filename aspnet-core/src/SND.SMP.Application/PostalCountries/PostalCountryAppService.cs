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
using SND.SMP.PostalCountries.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using OfficeOpenXml;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Abp.UI;

namespace SND.SMP.PostalCountries
{
    public class PostalCountryAppService(IRepository<PostalCountry, long> repository) : AsyncCrudAppService<PostalCountry, PostalCountryDto, long, PagedPostalCountryResultRequestDto>(repository)
    {
        protected override IQueryable<PostalCountry> CreateFilteredQuery(PagedPostalCountryResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.PostalCode.Contains(input.Keyword) ||
                    x.CountryCode.Contains(input.Keyword));
        }

        private static DataTable ConvertToDatatable(Stream ms)
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

        public override async Task<PostalCountryDto> CreateAsync(PostalCountryDto input)
        {
            CheckCreatePermission();

            var postalCountry = await Repository.FirstOrDefaultAsync(x => x.CountryCode.Equals(input.CountryCode));

            if (postalCountry is not null) throw new UserFriendlyException("Postal Country Exists");

            return await base.CreateAsync(input);
        }

        public async Task<List<string>> GetCountries()
        {
            var postalCountries = await Repository.GetAllListAsync();

            var distinctedCountries = postalCountries.DistinctBy(x => x.CountryCode);

            List<string> countries = [];

            foreach (var dc in distinctedCountries) countries.Add(dc.CountryCode);

            return countries;
        }

        [Consumes("multipart/form-data")]
        public async Task<List<PostalCountry>> UploadPostalCountryFile([FromForm] UploadPostalCountry input)
        {
            if (input.file == null || input.file.Length == 0) return [];

            DataTable dataTable = ConvertToDatatable(input.file.OpenReadStream());

            List<PostalCountryExcel> postalCountryExcel = [];
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr.ItemArray[0].ToString() != "")
                {
                    postalCountryExcel.Add(new PostalCountryExcel()
                    {
                        PostalCode = dr.ItemArray[0].ToString(),
                        CountryCode = dr.ItemArray[1].ToString(),
                    });
                }
            }

            await Repository.GetDbContext().Database.ExecuteSqlRawAsync("TRUNCATE TABLE tfsdb.postalcountries");

            List<PostalCountry> postalCountries = [];
            foreach (PostalCountryExcel excelItem in postalCountryExcel)
            {
                var insert = await Repository.InsertAsync(new PostalCountry()
                {
                    PostalCode = excelItem.PostalCode,
                    CountryCode = excelItem.CountryCode,
                });

                postalCountries.Add(insert);
            }

            return postalCountries;
        }
    }
}
