using Abp.Application.Services.Dto;
using System;

namespace SND.SMP.Customers
{

    public class PagedCustomerResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Code { get; set; }
        public string CompanyName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string RegistrationNo { get; set; }
        public string EmailAddress2 { get; set; }
        public string EmailAddress3 { get; set; }
        public bool? IsActive { get; set; }
    }
}
