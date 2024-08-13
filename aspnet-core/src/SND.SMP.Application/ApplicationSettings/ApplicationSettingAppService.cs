using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.ApplicationSettings.Dto;
using Microsoft.AspNetCore.Mvc;

namespace SND.SMP.ApplicationSettings
{
    public class ApplicationSettingAppService(IRepository<ApplicationSetting, int> repository) : AsyncCrudAppService<ApplicationSetting, ApplicationSettingDto, int, PagedApplicationSettingResultRequestDto>(repository)
    {
        protected override IQueryable<ApplicationSetting> CreateFilteredQuery(PagedApplicationSettingResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => 
                    x.Name.Contains(input.Keyword) ||
                    x.Value.Contains(input.Keyword));
        }

        public async Task<string> GetValueByName(string name)
        {
            var setting = await Repository.FirstOrDefaultAsync(x => x.Name.Equals(name));
            if (setting == null) return string.Empty;
            return setting.Value;
        }

        [HttpPost]
        public async Task<bool> UpdateValueByName(string name, string value)
        {
            var setting = await Repository.FirstOrDefaultAsync(x => x.Name.Equals(name));
            if (setting == null) return false;
            setting.Value = value;
            await Repository.UpdateAsync(setting);
            return true;
        }
    }
}
