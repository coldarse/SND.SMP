using System.Threading.Tasks;
using Abp.Application.Services;
using SND.SMP.Authorization.Accounts.Dto;

namespace SND.SMP.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
