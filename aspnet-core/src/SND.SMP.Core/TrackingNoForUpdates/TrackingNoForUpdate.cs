using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SND.SMP.TrackingNoForUpdates
{

    public class TrackingNoForUpdate : Entity<long>
    {
        public string TrackingNo { get; set; }
        public string DispatchNo { get; set; }
        public string ProcessType { get; set; }
    }
}
