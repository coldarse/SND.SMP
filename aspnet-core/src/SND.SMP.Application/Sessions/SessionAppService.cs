using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Domain.Repositories;
using SND.SMP.Customers;
using SND.SMP.Customers.Dto;
using SND.SMP.Sessions.Dto;

namespace SND.SMP.Sessions
{
    public class SessionAppService(IRepository<Customer, long> customerRepository) : SMPAppServiceBase, ISessionAppService
    {
        private readonly IRepository<Customer, long> _customerRepository = customerRepository;

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            var output = new GetCurrentLoginInformationsOutput
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>()
                }
            };

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {
                output.User = ObjectMapper.Map<UserLoginInfoDto>(await GetCurrentUserAsync());
            }


            return output;
        }
    }
}
