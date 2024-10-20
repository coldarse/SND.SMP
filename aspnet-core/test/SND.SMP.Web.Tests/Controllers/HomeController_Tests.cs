using System.Threading.Tasks;
using SND.SMP.Models.TokenAuth;
using SND.SMP.Web.Controllers;
using Shouldly;
using Xunit;

namespace SND.SMP.Web.Tests.Controllers
{
    public class HomeController_Tests: SMPWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}