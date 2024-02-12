using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using SND.SMP.Authorization;

namespace SND.SMP
{
    [DependsOn(
        typeof(SMPCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class SMPApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<SMPAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(SMPApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
