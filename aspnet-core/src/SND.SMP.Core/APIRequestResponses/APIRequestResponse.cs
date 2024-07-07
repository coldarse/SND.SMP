using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.APIRequestResponses
{

    public class APIRequestResponse : Entity<long>
    {
        public string URL { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime RequestDateTime { get; set; }
        public DateTime ResponseDateTime { get; set; }
        public int Duration { get; set; }
    }
}
