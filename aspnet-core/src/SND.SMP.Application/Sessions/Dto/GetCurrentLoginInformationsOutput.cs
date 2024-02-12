using SND.SMP.Customers.Dto;

namespace SND.SMP.Sessions.Dto
{
    public class GetCurrentLoginInformationsOutput
    {
        public ApplicationInfoDto Application { get; set; }

        public UserLoginInfoDto User { get; set; }

        public TenantLoginInfoDto Tenant { get; set; }

        public CustomerDto Customer { get; set; }
    }
}
