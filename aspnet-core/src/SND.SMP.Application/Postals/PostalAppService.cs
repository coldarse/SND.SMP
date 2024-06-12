using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Postals.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using OfficeOpenXml;
using SND.SMP.PostalOrgs;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace SND.SMP.Postals
{
    public class PostalAppService(IRepository<Postal, long> repository, IRepository<PostalOrg, string> postalOrgRepository) : AsyncCrudAppService<Postal, PostalDto, long, PagedPostalResultRequestDto>(repository)
    {
        protected override IQueryable<Postal> CreateFilteredQuery(PagedPostalResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.PostalCode.Contains(input.Keyword) ||
                    x.PostalDesc.Contains(input.Keyword) ||
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ServiceDesc.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.ProductDesc.Contains(input.Keyword));
        }

        public async Task<List<PostalDDL>> GetPostalDDL()
        {
            var postals = await Repository.GetAllListAsync();
            postals = postals.DistinctBy(x => x.PostalCode).ToList();

            List<PostalDDL> postalDDL = [];
            foreach (var postal in postals.ToList())
            {
                postalDDL.Add(new PostalDDL()
                {
                    PostalCode = postal.PostalCode,
                    PostalDesc = postal.PostalDesc
                });
            }
            return postalDDL;
        }

        public async Task<List<ServiceDDL>> GetServicesByPostal(string postalCode)
        {
            var postals = await Repository.GetAllListAsync(x => x.PostalCode.Equals(postalCode));
            postals = postals.DistinctBy(x => x.ServiceCode).ToList();

            List<ServiceDDL> serviceDDLs = [];
            foreach (Postal postal in postals.ToList())
            {
                serviceDDLs.Add(new ServiceDDL()
                {
                    ServiceCode = postal.ServiceCode,
                    ServiceDesc = postal.ServiceDesc
                });
            }
            return serviceDDLs;
        }

        public async Task<List<ProductDDL>> GetProductsByPostalAndService(string postalCode, string serviceCode)
        {
            var postals = await Repository.GetAllListAsync(x => x.PostalCode.Equals(postalCode) && x.ServiceCode.Equals(serviceCode));
            postals = postals.DistinctBy(x => x.ProductCode).ToList();

            List<ProductDDL> productDDLs = [];
            foreach (Postal postal in postals.ToList())
            {
                productDDLs.Add(new ProductDDL()
                {
                    ProductCode = postal.ProductCode,
                    ProductDesc = postal.ProductDesc
                });
            }
            return productDDLs;
        }

        public async Task<List<ProductDDL>> GetProductDDL()
        {
            var postals = await Repository.GetAllListAsync();
            postals = postals.DistinctBy(x => x.ProductCode).ToList();

            List<ProductDDL> productDDLs = [];
            foreach (Postal postal in postals.ToList())
            {
                productDDLs.Add(new ProductDDL()
                {
                    ProductCode = postal.ProductCode,
                    ProductDesc = postal.ProductDesc
                });
            }
            return productDDLs;
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
        public async Task<List<Postal>> UploadPostalFile([FromForm] UploadPostal input)
        {
            var postalOrganizations = await postalOrgRepository.GetAllListAsync();

            if (input.file == null || input.file.Length == 0) return [];

            DataTable dataTable = await ConvertToDatatable(input.file.OpenReadStream());

            List<PostalExcel> postalExcel = [];
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr.ItemArray[0].ToString() != "")
                {
                    postalExcel.Add(new PostalExcel()
                    {
                        PostalCode = dr.ItemArray[0].ToString(),
                        PostalDesc = dr.ItemArray[1].ToString(),
                        ServiceDesc = dr.ItemArray[2].ToString(),
                        ServiceCode = dr.ItemArray[3].ToString(),
                        ProductDesc = dr.ItemArray[4].ToString(),
                        ProductCode = dr.ItemArray[5].ToString(),
                        ItemTopUpValue = dr.ItemArray[6].ToString() == "" ? 0 : Convert.ToDecimal(dr.ItemArray[6]),
                    });
                }
            }

            var distinctedByPostalCode = postalExcel.DistinctBy(x => x.PostalCode?[0..Math.Min(x.PostalCode.Length, 2)]);

            List<PostalOrg> postalOrg = [];
            foreach (var distinctedPostalCode in distinctedByPostalCode)
            {
                string subStringedPostalCode = distinctedPostalCode.PostalCode?[0..Math.Min(distinctedPostalCode.PostalCode.Length, 2)];
                var splitPostalDesc = distinctedPostalCode.PostalDesc.Split("-");

                var org = postalOrganizations.FirstOrDefault(x => x.Id.Equals(subStringedPostalCode));

                if (org is null)
                {
                    var postalOrgCreate = await postalOrgRepository.InsertAsync(new PostalOrg()
                    {
                        Id = subStringedPostalCode,
                        Name = splitPostalDesc[0].ToString().TrimEnd()
                    });
                }
            }

            await Repository.GetDbContext().Database.ExecuteSqlRawAsync("TRUNCATE TABLE smpdb.postals");

            List<Postal> postals = [];
            foreach (PostalExcel excelItem in postalExcel.ToList())
            {
                var insert = await Repository.InsertAsync(new Postal()
                {
                    PostalCode = excelItem.PostalCode,
                    PostalDesc = excelItem.PostalDesc,
                    ServiceCode = excelItem.ServiceCode,
                    ServiceDesc = excelItem.ServiceDesc,
                    ProductCode = excelItem.ProductCode,
                    ProductDesc = excelItem.ProductDesc,
                    ItemTopUpValue = excelItem.ItemTopUpValue,
                });

                postals.Add(insert);
            }

            return postals;
        }

    }
}
