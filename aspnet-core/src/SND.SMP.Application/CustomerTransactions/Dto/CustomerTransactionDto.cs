﻿using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SND.SMP.CustomerTransactions.Dto
{
    [AutoMap(typeof(CustomerTransaction))]
    public class CustomerTransactionDto : EntityDto<long>
    {
        public string Wallet { get; set; }
        public string Customer { get; set; }
        public string PaymentMode { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
