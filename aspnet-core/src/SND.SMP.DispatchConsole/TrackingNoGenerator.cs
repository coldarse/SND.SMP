using SND.SMP.DispatchConsole.EF;
using static SND.SMP.Shared.EnumConst;
using OfficeOpenXml;
using System.Net.Http.Headers;
using System.Text.Json;
using SND.SMP.Chibis;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace SND.SMP.DispatchConsole
{
    public class TrackingNoGenerator
    {
        private int ApplicationId { get; set; }
        private string Customer { get; set; }
        private string Prefix { get; set; }
        private string PrefixNo { get; set; }
        private string Suffix { get; set; }
        private int RunningNo { get; set; }
        private int AmountRequested { get; set; }
        private int AmountGiven { get; set; }

        public TrackingNoGenerator() { }

        public async Task DiscoverAndGenerate()
        {
            using db db = new();
            var hasGenerating = db.ItemTrackingApplications
                    .Where(x => x.Status == GenerateConst.Status_Generating)
                    .Any();

            if (hasGenerating) return;

            var application = db.ItemTrackingApplications
                    .Where(x => x.Status == GenerateConst.Status_Approved)
                    .OrderBy(x => x.DateCreated)
                    .FirstOrDefault();

            if (application is not null)
            {
                ApplicationId = application.Id;

                application.Status = GenerateConst.Status_Generating;

                await db.SaveChangesAsync().ConfigureAwait(false);
            }

            var review = db.ItemTrackingReviews.FirstOrDefault(x => x.ApplicationId.Equals(ApplicationId));

            if (review is not null)
            {

                DateTime startTime = DateTime.Now;
                Customer = review.CustomerCode;
                Prefix = review.Prefix;
                PrefixNo = review.PrefixNo;
                Suffix = string.IsNullOrWhiteSpace(review.Suffix) ? "" : review.Suffix ;
                AmountRequested = review.Total;

                var runningNos = db.ItemIdRunningNos
                        .Where(x => x.Customer.Equals(Customer))
                        .Where(x => x.Prefix.Equals(Prefix))
                        .Where(x => x.PrefixNo.Equals(PrefixNo))
                        .Where(x => x.Suffix.Equals(Suffix))
                        .FirstOrDefault();

                if (runningNos is not null) RunningNo = runningNos.RunningNo + 1;
                else
                {
                    RunningNo = 0;
                    await db.ItemIdRunningNos.AddAsync(new ItemIdRunningNos.ItemIdRunningNo
                    {
                        Customer = Customer,
                        Prefix = Prefix,
                        PrefixNo = PrefixNo.ToString(),
                        Suffix = Suffix,
                        RunningNo = 0,
                    }).ConfigureAwait(false);

                    await db.SaveChangesAsync();

                    runningNos = db.ItemIdRunningNos
                        .Where(x => x.Customer.Equals(Customer))
                        .Where(x => x.Prefix.Equals(Prefix))
                        .Where(x => x.PrefixNo.Equals(PrefixNo))
                        .Where(x => x.Suffix.Equals(Suffix))
                        .FirstOrDefault();
                }

                int startingNo = RunningNo;
                int endingNo = startingNo + AmountRequested;
                int maxSerialNo = 9999999;

                if (endingNo > maxSerialNo)
                {
                    int over = endingNo - maxSerialNo;
                    AmountGiven = maxSerialNo - startingNo;
                    review.Remark = $"The amount requested is over by {over}. Only able to generate {AmountGiven} Tracking Ids.";
                    review.TotalGiven = AmountGiven;
                    endingNo = maxSerialNo;
                }
                else
                {
                    AmountGiven = AmountRequested;
                    review.TotalGiven = AmountGiven;
                }

                List<string> trackingIds = [];

                for (var i = 0; i < AmountGiven; i++)
                {
                    int maxDigitLength = 7;

                    int padLeft = maxDigitLength - PrefixNo.ToString().Length;

                    string serialNo = PrefixNo + Convert.ToString(startingNo + i).PadLeft(padLeft, '0');

                    bool add = true;

                    int checkDigit = GenerateCheckDigit(serialNo);

                    string trackingId = string.Format("{0}{1}{2}{3}", Prefix, serialNo, checkDigit.ToString(), Suffix);

                    var checkExistenceEnabled = false;
                    if (checkExistenceEnabled)
                    {
                        if (AmountRequested == 1)
                        {
                            var existed = await IsTrackingNumberExist(trackingId, Prefix, PrefixNo.ToString(), Suffix);
                            if (existed)
                            {
                                serialNo = PrefixNo + Convert.ToString(startingNo + i + 1).PadLeft(padLeft, '0');
                                checkDigit = GenerateCheckDigit(serialNo);
                                trackingId = string.Format("{0}{1}{2}{3}", Prefix, serialNo, checkDigit.ToString(), Suffix);

                                existed = await IsTrackingNumberExist(trackingId, Prefix, PrefixNo.ToString(), Suffix);

                                if (existed)
                                {
                                    add = false;
                                    review.Remark = "Tracking Id already Exists.";
                                    review.Status = GenerateConst.Status_Declined;
                                }
                            }
                        }
                    }

                    if (add) trackingIds.Add(trackingId);
                }

                if (trackingIds.Count == 0)
                {
                    review.Status = GenerateConst.Status_Declined;

                    application.Path = "";
                    application.Range = "";
                    application.Status = GenerateConst.Status_Declined;
                }
                else
                {
                    byte[] buffer = CreateTrackingIdExcelFile(trackingIds, review.DateCreated.ToShortDateString());

                    Stream stream = new MemoryStream(buffer);

                    string fileName = string.Format("{0}_{1}_{2}_{3}_{4}.xlsx", Prefix, PrefixNo, Suffix, AmountGiven, Customer.Replace(" ", "_"));

                    ChibiUpload uploadExcel = await FileServer.InsertExcelFileToChibi(stream, fileName, originalName: null, postalCode: review.PostalCode, productCode: review.ProductCode);

                    application.Path = uploadExcel.url;
                    application.Range = string.Format("{0} - {1}", trackingIds[0].ToString(), trackingIds[^1].ToString());
                    application.Status = GenerateConst.Status_Completed;
                }

                runningNos.RunningNo = endingNo;

                DateTime endTime = DateTime.Now;
                application.TookInSec = (endTime - startTime).Seconds;

                await db.SaveChangesAsync().ConfigureAwait(false);

            }
        }


        private static byte[] CreateTrackingIdExcelFile(List<string> trackingIds, string dateCreated)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Tracking Numbers");

            string[] headers = ["TrackingNo", "DateCreated", "DateUsed", "DispatchNo"];

            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = headers[col];
            }

            int recordIndex = 2;

            foreach (var trackingId in trackingIds)
            {
                worksheet.Cells[recordIndex, 1].Value = trackingId;
                worksheet.Cells[recordIndex, 2].Value = dateCreated;
                recordIndex++;
            }

            return package.GetAsByteArray();
        }

        public static async Task<bool> IsTrackingNumberExist(string trackingNo, string Prefix, string PrefixNo, string Suffix)
        {
            using db db = new();

            var reviews = db.ItemTrackingReviews
                .Where(x => x.Prefix == Prefix)
                .Where(x => x.PrefixNo == PrefixNo)
                .Where(x => x.Suffix == Suffix)
                .ToList();

            if (reviews.Count == 0) return false;

            foreach (var review in reviews)
            {
                var applications = db.ItemTrackingApplications.Where(x => x.Id.Equals(review.ApplicationId)).ToList();

                foreach (var application in applications)
                {
                    Stream excelFile = await FileServer.GetFileStream(application.Path);

                    DataTable dataTable = ConvertToDatatable(excelFile);

                    if (dataTable.Rows.Count == 0) return false;

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (dr.ItemArray[0].ToString() == trackingNo) return true;
                    }
                }

            }

            return false;
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

        private static bool IsCheckDigitValid(string trackingNo)
        {
            bool valid;

            string serialNo = trackingNo.Substring(2, 8);
            int checkDigit = Convert.ToInt32(trackingNo.Substring(10, 1));

            valid = GenerateCheckDigit(serialNo) == checkDigit;

            return valid;
        }

        private static int GenerateCheckDigit(string serialNo)
        {
            int[] multiplier = [8, 6, 4, 2, 3, 5, 9, 7];

            char[] charArr = serialNo.ToCharArray();
            int checkDigit;
            int sum = 0;

            for (int i = 0; i < charArr.Length; i++)
            {
                int x = int.Parse(charArr[i].ToString());
                int m = multiplier[i];

                sum += x * m;
            }

            int remainder = sum % 11;

            if (remainder == 0) checkDigit = 5;
            else if (remainder == 1) checkDigit = 0;
            else checkDigit = 11 - remainder;

            return checkDigit;
        }
    }
}
