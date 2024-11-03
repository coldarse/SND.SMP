using System;
namespace SND.SMP.DispatchImporter.Dto
{
	public class DispatchValidateDto
	{
		public string Category { get; set; }
		public List<string> ItemIds { get; set; } = [];
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

	public class DispatchValidateIOSSDto
	{
		public int Row { get; set; }
		public string TrackingNo { get; set; }
		public string CountryCode { get; set; }
		public string IOSS { get; set; }
	}
}

