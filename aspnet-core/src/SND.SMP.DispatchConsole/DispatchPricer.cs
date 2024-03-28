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

		public string CurrencyId { get; set; }

		public DispatchPricer(string accNo, string postalCode, string serviceCode, string productCode, string rateOptionId, string paymentMode)
		{
			_accNo = accNo;
			_postalCode = postalCode;
			_serviceCode = serviceCode;
			_productCode = productCode;
			_rateOptionId = rateOptionId;
			_paymentMode = paymentMode;

			_useRateMaintenance = _serviceCode == DispatchEnumConst.SERVICE_TS;
			_useRateWeightBreak = _serviceCode == DispatchEnumConst.SERVICE_DE;

			using (EF.db db = new EF.db())
			{
				if (_useRateMaintenance)
				{
					_rates = db.Customerpostals
						.Where(u => u.CustomerCode == _accNo)
						.Where(u => u.PostalCode == _postalCode)
						.SelectMany(u => u.Rate.Rateitems)
						.Where(u => u.ServiceCode == _serviceCode)
						.Where(u => u.ProductCode == _productCode)
						.Where(u => u.PaymentMode == _paymentMode)
						.ToList();

					this.CurrencyId = _rates.Select(u => u.CurrencyId).FirstOrDefault();
				}

				if (_useRateWeightBreak)
				{
					_rateWeightBreaks = db.Customerpostals
						.Where(u => u.CustomerCode == _accNo)
						.Where(u => u.PostalCode == _postalCode)
						.SelectMany(u => u.Rate.Rateweightbreaks)
						.Where(u => u.ProductCode == _productCode)
						.Where(u => u.PaymentMode == _paymentMode)
						.ToList();

					this.CurrencyId = _rateWeightBreaks.Select(u => u.CurrencyId).FirstOrDefault();
				}
			}
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
						.Select(u => new { Total = u.Total, RegisteredFee = u.RegisteredFee })
						.FirstOrDefault();

					if (rate != null)
					{
						price = Math.Round(rate.Total.GetValueOrDefault() * weight, 2) + rate.RegisteredFee.GetValueOrDefault();
					}
				}
			}

			if (_useRateWeightBreak)
			{
				var rate = _rateWeightBreaks
					.Where(u => u.PostalOrgId == countryCode)
					.Where(u => weight >= u.WeightMin && weight <= u.WeightMax)
					.Select(u => new { ItemRate = u.ItemRate, WeightRate = u.WeightRate })
					.FirstOrDefault();

				if (rate != null)
				{
					price = rate.ItemRate.GetValueOrDefault() + Math.Round(rate.WeightRate.GetValueOrDefault() * weight, 2);
				}
				else
				{
					var rateHeaviest = _rateWeightBreaks
						.Where(u => u.PostalOrgId == countryCode)
						.OrderByDescending(u => u.WeightMax)
						.Select(u => new { WeightMax = u.WeightMax, ItemRate = u.ItemRate, WeightRate = u.WeightRate })
						.FirstOrDefault();

					if (rateHeaviest != null)
					{
						if (weight > rateHeaviest.WeightMax)
						{
							var rateExceed = _rateWeightBreaks
								.Where(u => u.PostalOrgId == countryCode)
								.Where(u => u.IsExceedRule == 1)
								.Select(u => new { ItemRate = u.ItemRate, WeightRate = u.WeightRate })
								.FirstOrDefault();

							if (rateExceed != null)
							{
								var weightExceed = Math.Round(weight - rateHeaviest.WeightMax.GetValueOrDefault(), 3);

								price = rateHeaviest.ItemRate.GetValueOrDefault() + Math.Round(rateHeaviest.WeightRate.GetValueOrDefault() * Math.Round(rateHeaviest.WeightMax.GetValueOrDefault(), 3), 2);
								price += rateExceed.ItemRate.GetValueOrDefault() + Math.Round(rateExceed.WeightRate.GetValueOrDefault() * weightExceed, 2);
							}
						}
					}
				}
			}

			return price;
		}
	}
}

