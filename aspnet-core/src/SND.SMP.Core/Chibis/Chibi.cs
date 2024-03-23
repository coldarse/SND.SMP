using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.Chibis
{

    public class Chibi : Entity<long>
    {
        public string FileName { get; set; }
        public string UUID { get; set; }
        public string URL { get; set; }
        public string OriginalName { get; set; }
        public string GeneratedName { get; set; }
    }
}
