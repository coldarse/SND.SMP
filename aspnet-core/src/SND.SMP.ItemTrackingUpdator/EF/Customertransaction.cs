using System;
using System.Collections.Generic;

namespace SND.SMP.ItemTrackingUpdater.EF;

public partial class Customertransaction
{
    public uint Id { get; set; }

    public string CustomerCode { get; set; }

    public string TransactionType { get; set; }

    public decimal? Amount { get; set; }

    public string CurrencyId { get; set; }

    public string RefNo { get; set; }

    public string Description { get; set; }

    public DateTime? DateTransaction { get; set; }

    public virtual Customer CustomerCodeNavigation { get; set; }
}
