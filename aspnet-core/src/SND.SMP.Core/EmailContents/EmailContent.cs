using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.EmailContents
{

    public class EmailContent : Entity<int>
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
