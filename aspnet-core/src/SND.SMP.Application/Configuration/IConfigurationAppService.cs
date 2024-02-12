using System.Threading.Tasks;
using SND.SMP.Configuration.Dto;

namespace SND.SMP.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
