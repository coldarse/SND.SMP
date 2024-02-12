using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace SND.SMP.Controllers
{
    public abstract class SMPControllerBase: AbpController
    {
        protected SMPControllerBase()
        {
            LocalizationSourceName = SMPConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
