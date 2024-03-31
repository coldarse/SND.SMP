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
using SND.SMP.Bags.Dto;

namespace SND.SMP.Bags
{
    public class BagAppService : AsyncCrudAppService<Bag, BagDto, int, PagedBagResultRequestDto>
    {

        public BagAppService(IRepository<Bag, int> repository) : base(repository)
        {
        }
        protected override IQueryable<Bag> CreateFilteredQuery(PagedBagResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.BagNo                         .Contains(input.Keyword) ||
                    x.CountryCode                   .Contains(input.Keyword) ||
                    x.CN35No                        .Contains(input.Keyword));
        }
    }
}
