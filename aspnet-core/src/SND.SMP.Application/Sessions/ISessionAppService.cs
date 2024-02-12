using System.Threading.Tasks;
using Abp.Application.Services;
using SND.SMP.Sessions.Dto;

namespace SND.SMP.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
