using System;
using static SND.SMP.Shared.EnumConst;

namespace SND.SMP.DispatchConsole
{
	public class DispatchPricer
	{
		private string _accNo { get; set; }
		private string _postalCode { get; set; }
		private string _serviceCode { get; set; }
		private string _productCode { get; set; }
		private string _rateOptionId { get; set; }
		private string _paymentMode { get; set; }

		private bool _useRateMaintenance { get; set; }
		private bool _useRateWeightBreak { get; set; }

		private List<EF.Rateitem> _rates { get; set; } = null;
		private List<EF.Rateweightbreak> _rateWeightBreaks { get; set; } = null;

		public int CurrencyId { get; set; }

		public string ErrorMsg { get; set; }

		public DispatchPricer(string accNo, string postalCode, string serviceCode, string productCode, string rateOptionId, string paymentMode)
		{
			ErrorMsg = "";
			_accNo = accNo;
			_postalCode = postalCode;
			_serviceCode = serviceCode;
			_productCode = productCode;
			_rateOptionId = rateOptionId;
			_paymentMode = paymentMode;

			_useRateMaintenance = _serviceCode == DispatchEnumConst.SERVICE_TS;
			_useRateWeightBreak = _serviceCode == DispatchEnumConst.SERVICE_DE;

			using EF.db db = new();
			var acctNo = db.Customers.FirstOrDefault(u => u.Code == _accNo);
			var _customerPostal = db.Customerpostals.FirstOrDefault(u => (u.AccountNo == acctNo.Id) && (u.Postal == _postalCode));

			if (_customerPostal is not null)
			{
				if (_useRateMaintenance)
				{
					// _rates = db.Customerpostals
					// 	.Where(u => u.AccountNo == _accNo)
					// 	.Where(u => u.Postal == _postalCode)
					// 	.SelectMany(u => u.RateNav.Rateitems)
					// 	.Where(u => u.ServiceCode == _serviceCode)
					// 	.Where(u => u.ProductCode == _productCode)
					// 	.Where(u => u.PaymentMode == _paymentMode)
					// 	.ToList();

					_rates = [.. db.Rateitems
							.Where(u => u.RateId == _customerPostal.Rate)
							.Where(u => u.ProductCode == _productCode)
							.Where(u => u.ServiceCode == _serviceCode)];

					if (_rates.Count > 0)
					{
						CurrencyId = _rates.Select(u => u.CurrencyId).FirstOrDefault();

						if (CurrencyId == 0) ErrorMsg = $"Unable to find Currency with the Id of {_rates.First().CurrencyId}";
					}
					else
					{
						var rateCard = db.Rates.FirstOrDefault(u => u.Id.Equals(_customerPostal.Rate));
						string rateValue = rateCard is null ? _customerPostal.Rate.ToString() : rateCard.CardName;
						ErrorMsg = $"No rates found for this Rate Id of {rateValue}";
					}
				}

				if (_useRateWeightBreak)
				{
					// _rateWeightBreaks = db.Customerpostals
					// 	.Where(u => u.AccountNo == _accNo)
					// 	.Where(u => u.Postal == _postalCode)
					// 	.SelectMany(u => u.RateNav.Rateweightbreaks)
					// 	.Where(u => u.ProductCode == _productCode)
					// 	.Where(u => u.PaymentMode == _paymentMode)
					// 	.ToList();

					_rateWeightBreaks = [.. db.Rateweightbreaks
										.Where(u => u.RateId == _customerPostal.Rate)
										.Where(u => u.ProductCode == _productCode)];

					if (_rateWeightBreaks.Count > 0)
					{
						CurrencyId = _rateWeightBreaks.Select(u => u.CurrencyId).FirstOrDefault();

						if (CurrencyId == 0) ErrorMsg = $"Unable to find Currency with the Id of {_rateWeightBreaks.First().CurrencyId}";
					}
					else
					{
						var rateCard = db.Rates.FirstOrDefault(u => u.Id.Equals(_customerPostal.Rate));
						string rateValue = rateCard is null ? _customerPostal.Rate.ToString() : rateCard.CardName;
						ErrorMsg = $"No rates found for this Rate Id of {rateValue}";
					}

				}
			}
			else ErrorMsg = "No Customer Postal exists for this Dispatch.";

		}

		public decimal CalculatePrice(string countryCode, decimal weight)
		{
			var price = 0m;

			if (_useRateMaintenance)
			{
				if (_rates != null)
				{
					var rate = _rates
						.Where(u => u.CountryCode == countryCode)
						.Select(u => new { u.Total, RegisteredFee = u.Fee })
						.FirstOrDefault();

					if (rate != null)
					{
						price = rate.Total.GetValueOrDefault() * weight + rate.RegisteredFee.GetValueOrDefault();
					}
					else ErrorMsg = $"No Rate found for Country Code: {countryCode} with Service Code: {_serviceCode} and Product Code: {_productCode}. Please contact System Finance for further review.";
				}
			}

			if (_useRateWeightBreak)
			{
				var rate = _rateWeightBreaks
					.Where(u => u.PostalOrgId == countryCode)
					.Where(u => weight >= u.WeightMin && weight <= u.WeightMax)
					.Select(u => new { u.ItemRate, u.WeightRate })
					.FirstOrDefault();

				if (rate != null)
				{
					price = rate.ItemRate.GetValueOrDefault() + rate.WeightRate.GetValueOrDefault() * weight;
				}
				else
				{
					var rateHeaviest = _rateWeightBreaks
						.Where(u => u.PostalOrgId == countryCode)
						.OrderByDescending(u => u.WeightMax)
						.Select(u => new { u.WeightMax, u.ItemRate, u.WeightRate })
						.FirstOrDefault();

					if (rateHeaviest != null)
					{
						if (weight > rateHeaviest.WeightMax)
						{
							var rateExceed = _rateWeightBreaks
								.Where(u => u.PostalOrgId == countryCode)
								.Where(u => u.IsExceedRule == 1)
								.Select(u => new { u.ItemRate, u.WeightRate })
								.FirstOrDefault();

							if (rateExceed != null)
							{
								var weightExceed = weight - rateHeaviest.WeightMax.GetValueOrDefault();

								price = rateHeaviest.ItemRate.GetValueOrDefault() + rateHeaviest.WeightRate.GetValueOrDefault() * rateHeaviest.WeightMax.GetValueOrDefault();
								price += rateExceed.ItemRate.GetValueOrDefault() + rateExceed.WeightRate.GetValueOrDefault() * weightExceed;
							}
						}
					}
					else ErrorMsg = $"No Rate found for Country Code: {countryCode} with Service Code: {_serviceCode} and Product Code: {_productCode}. Please contact System Finance for further review.";
				}
			}

			return price;
		}
	}
}

