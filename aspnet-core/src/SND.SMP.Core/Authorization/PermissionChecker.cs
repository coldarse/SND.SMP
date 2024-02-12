using Abp.Authorization;
using SND.SMP.Authorization.Roles;
using SND.SMP.Authorization.Users;

namespace SND.SMP.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
