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

namespace SND.SMP.Wallets
{
    public class WalletAppService : AsyncCrudAppService<Wallet, WalletDto, custom, PagedWalletResultRequestDto>
    {

        public WalletAppService(IRepository<Wallet, custom> repository) : base(repository)
        {
        }
        protected override IQueryable<Wallet> CreateFilteredQuery(PagedWalletResultRequestDto input)
        {
            return Repository.GetAllIncluding().AsQueryable();
        }
    }
}
