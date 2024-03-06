using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.PostalOrgs
{

    public class PostalOrg : Entity<string>
    {
        public string Name { get; set; }
    }
}
