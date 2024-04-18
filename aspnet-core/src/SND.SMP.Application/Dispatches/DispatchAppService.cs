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
using SND.SMP.Dispatches.Dto;
using SND.SMP.Customers;
using SND.SMP.Bags;
using Abp.UI;

namespace SND.SMP.Dispatches
{
    public class DispatchAppService : AsyncCrudAppService<Dispatch, DispatchDto, int, PagedDispatchResultRequestDto>
    {

        public readonly IRepository<Customer, long> _customerRepository;
        public readonly IRepository<Bag, int> _bagRepository;
        public DispatchAppService(IRepository<Dispatch, int> repository, IRepository<Customer, long> customerRepository, IRepository<Bag, int> bagRepository) : base(repository)
        {
            _customerRepository = customerRepository;
            _bagRepository = bagRepository;
        }
        protected override IQueryable<Dispatch> CreateFilteredQuery(PagedDispatchResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.CustomerCode.Contains(input.Keyword) ||
                    x.POBox.Contains(input.Keyword) ||
                    x.PPI.Contains(input.Keyword) ||
                    x.PostalCode.Contains(input.Keyword) ||
                    x.ServiceCode.Contains(input.Keyword) ||
                    x.ProductCode.Contains(input.Keyword) ||
                    x.DispatchNo.Contains(input.Keyword) ||
                    x.FlightTrucking.Contains(input.Keyword) ||
                    x.BatchId.Contains(input.Keyword) ||
                    x.CN38.Contains(input.Keyword) ||
                    x.Remark.Contains(input.Keyword) ||
                    x.AirlineCode.Contains(input.Keyword) ||
                    x.FlightNo.Contains(input.Keyword) ||
                    x.PortDeparture.Contains(input.Keyword) ||
                    x.ExtDispatchNo.Contains(input.Keyword) ||
                    x.AirportTranshipment.Contains(input.Keyword) ||
                    x.OfficeDestination.Contains(input.Keyword) ||
                    x.OfficeOrigin.Contains(input.Keyword) ||
                    x.Stage1StatusDesc.Contains(input.Keyword) ||
                    x.Stage2StatusDesc.Contains(input.Keyword) ||
                    x.Stage3StatusDesc.Contains(input.Keyword) ||
                    x.Stage4StatusDesc.Contains(input.Keyword) ||
                    x.Stage5StatusDesc.Contains(input.Keyword) ||
                    x.Stage6StatusDesc.Contains(input.Keyword) ||
                    x.Stage7StatusDesc.Contains(input.Keyword) ||
                    x.Stage8StatusDesc.Contains(input.Keyword) ||
                    x.Stage9StatusDesc.Contains(input.Keyword) ||
                    x.StatusAPI.Contains(input.Keyword) ||
                    x.CountryOfLoading.Contains(input.Keyword) ||
                    x.PostManifestMsg.Contains(input.Keyword) ||
                    x.PostDeclarationMsg.Contains(input.Keyword) ||
                    x.AirwayBLNo.Contains(input.Keyword) ||
                    x.BRCN38RequestId.Contains(input.Keyword) ||
                    x.CORateOptionId.Contains(input.Keyword) ||
                    x.PaymentMode.Contains(input.Keyword) ||
                    x.CurrencyId.Contains(input.Keyword));
        }


        public async Task<GetPostCheck> GetPostCheckAsync(string dispatchNo)
        {
            var dispatch = await Repository.FirstOrDefaultAsync(x => x.DispatchNo.Equals(dispatchNo)) ?? throw new UserFriendlyException("Dispatch Not Found.");

            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Code.Equals(dispatch.CustomerCode)) ?? throw new UserFriendlyException("Customer Not Found.");

            var bags = await _bagRepository.GetAllListAsync(x => x.DispatchId.Equals(dispatch.Id)) ?? throw new UserFriendlyException("No Bags Found.");

            return new GetPostCheck()
            {
                CompanyName = customer.CompanyName ?? "",
                CompanyCode = customer.Code ?? "",
                DispatchNo = dispatchNo ?? "",
                FlightTrucking = dispatch.FlightTrucking ?? "",
                ETA = dispatch.ETAtoHKG ?? DateOnly.FromDateTime(DateTime.Now),
                ATA = dispatch.ATA ?? DateTime.MinValue,
                PreCheckNoOfBag = dispatch.NoofBag ?? 0,
                PostCheckNoOfBag = dispatch.PostCheckTotalBags ?? 0,
                PreCheckWeight = dispatch.TotalWeight ?? Convert.ToDecimal(0),
                PostCheckWeight = dispatch.PostCheckTotalWeight ?? Convert.ToDecimal(0),
                Bags = bags ?? []
            };
        }
    }
}
