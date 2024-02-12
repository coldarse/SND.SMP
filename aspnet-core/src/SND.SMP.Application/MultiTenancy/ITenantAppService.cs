using Abp.Application.Services;
using SND.SMP.MultiTenancy.Dto;

namespace SND.SMP.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

