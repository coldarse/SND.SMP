using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SND.SMP.Roles.Dto;
using SND.SMP.Users.Dto;

namespace SND.SMP.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task DeActivate(EntityDto<long> user);
        Task Activate(EntityDto<long> user);
        Task<ListResultDto<RoleDto>> GetRoles();
        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);

        Task GetAndDeleteUserByUsername(string username);
    }
}
