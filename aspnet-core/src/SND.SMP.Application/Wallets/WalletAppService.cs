using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.Wallets.Dto;
using Abp.Application.Services.Dto;
using Abp.UI;
using SND.SMP.Currencies;
using System.Reflection.Metadata.Ecma335;

namespace SND.SMP.Wallets
{
    public class WalletAppService : AsyncCrudAppService<Wallet, WalletDto, string, PagedWalletResultRequestDto>
    {

        public WalletAppService(IRepository<Wallet, string> repository) : base(repository)
        {
        }
        protected override IQueryable<Wallet> CreateFilteredQuery(PagedWalletResultRequestDto input)
        {
            return Repository.GetAllIncluding()
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Customer.Contains(input.Keyword))
            .WhereIf(!input.Customer.IsNullOrWhiteSpace(), x => x.Customer.ToLower().Equals(input.Customer.ToLower()))
            .AsQueryable();
        }

        public override async Task<WalletDto> CreateAsync(WalletDto input)
        {
            var wallets = await Repository.GetAllListAsync(x => x.Customer.Equals(input.Customer));

            /* If Wallet does not exist */
            if(wallets.Count == 0) return await base.CreateAsync(input);
            /* If Wallet exist */
            else 
            {
                var customerWallet = wallets.Where(x => x.Customer.Equals(input.Customer));
                bool eWalletTypeExist = customerWallet.Any(x => x.EWalletType.Equals(input.EWalletType));
                /* If EwalletType does not exist */
                if (!eWalletTypeExist) return await base.CreateAsync(input);
                /* If EwalletType exist */
                else
                {
                    var customerWithEWalletType = customerWallet.Where(x => x.EWalletType.Equals(input.EWalletType));
                    bool currencyExist = customerWithEWalletType.Any(x => x.Currency.Equals(input.Currency));
                    /* If Currency does not exist */
                    if (!currencyExist) return await base.CreateAsync(input);
                    /* If Currency does not exist */
                    else throw new UserFriendlyException("You have already create a similar wallet. Please try again.");
                }
            }
        }

        public override async Task<PagedResultDto<WalletDto>> GetAllAsync(PagedWalletResultRequestDto input)
        {
            return await base.GetAllAsync(input);
        }

        public override async Task<WalletDto> GetAsync(EntityDto<string> input)
        {
            return await base.GetAsync(input);
        }

        public override async Task<WalletDto> UpdateAsync(WalletDto input)
        {
            return await base.UpdateAsync(input);
        }

        public override Task DeleteAsync(EntityDto<string> input)
        {
            return base.DeleteAsync(input);
        }

    }
}
