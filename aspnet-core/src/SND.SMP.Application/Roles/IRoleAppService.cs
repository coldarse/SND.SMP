using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SND.SMP.Authorization.Roles;
using SND.SMP.Roles.Dto;

namespace SND.SMP.Roles
{
    public interface IRoleAppService : IAsyncCrudAppService<RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>
    {
        Task<ListResultDto<PermissionDto>> GetAllPermissions();

        Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input);

        Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input);

        Task<Role> GetRoleByName(string role);
    }
}
