using System;
namespace SND.SMP.DispatchConsole.Dto
{
	public class DispatchValidateDto
	{
		public string Category { get; set; }
		public List<string> ItemIds { get; set; } = new List<string>();
		public string Message { get; set; }
	}

	public class DispatchValidatePostalCountryDto
	{
		public string CountryCode { get; set; }
	}

    public class DispatchValidateCountryDto
    {
		public string Id { get; set; }
        public string CountryCode { get; set; }
    }

	public class DispatchValidateParticularsDto
	{
		public string Id { get; set; }
		public string DispatchNo { get; set; }
		public string PostalCode { get; set; }
		public string ServiceCode { get; set; }
		public string ProductCode { get; set; }
	}
}

