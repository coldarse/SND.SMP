using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.ApplicationSettings
{

    public class ApplicationSetting : Entity<int>
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
