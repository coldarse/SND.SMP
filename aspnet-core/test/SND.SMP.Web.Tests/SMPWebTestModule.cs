using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using SND.SMP.EntityFrameworkCore;
using SND.SMP.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace SND.SMP.Web.Tests
{
    [DependsOn(
        typeof(SMPWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class SMPWebTestModule : AbpModule
    {
        public SMPWebTestModule(SMPEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SMPWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(SMPWebMvcModule).Assembly);
        }
    }
}