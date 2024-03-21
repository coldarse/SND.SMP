using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using SND.SMP.Authorization.Roles;
using SND.SMP.Authorization.Users;
using SND.SMP.MultiTenancy;
/* Using Definition */
using SND.SMP.RateWeightBreaks;
using SND.SMP.CustomerPostals;
using SND.SMP.PostalCountries;
using SND.SMP.PostalOrgs;
using SND.SMP.Postals;
using SND.SMP.RateItems;
using SND.SMP.Rates;
using SND.SMP.Wallets;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;
using SND.SMP.Customers;
using SND.SMP.CustomerTransactions;


namespace SND.SMP.EntityFrameworkCore
{
    public class SMPDbContext : AbpZeroDbContext<Tenant, Role, User, SMPDbContext>
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<EWalletType> EWalletTypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<RateItem> RateItems { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<Postal> Postals { get; set; }
        public DbSet<PostalOrg> PostalOrgs { get; set; }
        public DbSet<PostalCountry> PostalCountries { get; set; }
        public DbSet<CustomerPostal> CustomerPostals { get; set; }
        public DbSet<RateWeightBreak> RateWeightBreaks { get; set; }
        /* Define a DbSet for each entity of the application */

        public SMPDbContext(DbContextOptions<SMPDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* Define Tables */
            builder.Entity<RateWeightBreak>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "RateWeightBreaks");
                b.Property(x => x.RateId).HasColumnName(nameof(RateWeightBreak.RateId));
                b.Property(x => x.PostalOrgId).HasColumnName(nameof(RateWeightBreak.PostalOrgId));
                b.Property(x => x.WeightMin).HasColumnName(nameof(RateWeightBreak.WeightMin)).HasPrecision(18, 2);
                b.Property(x => x.WeightMax).HasColumnName(nameof(RateWeightBreak.WeightMax)).HasPrecision(18, 2);
                b.Property(x => x.ProductCode).HasColumnName(nameof(RateWeightBreak.ProductCode)).HasMaxLength(10);
                b.Property(x => x.CurrencyId).HasColumnName(nameof(RateWeightBreak.CurrencyId));
                b.Property(x => x.ItemRate).HasColumnName(nameof(RateWeightBreak.ItemRate)).HasPrecision(18, 2);
                b.Property(x => x.WeightRate).HasColumnName(nameof(RateWeightBreak.WeightRate)).HasPrecision(18, 2);
                b.Property(x => x.IsExceedRule).HasColumnName(nameof(RateWeightBreak.IsExceedRule));
                b.Property(x => x.PaymentMode).HasColumnName(nameof(RateWeightBreak.PaymentMode)).HasMaxLength(128);
                b.HasOne<Rate>().WithMany().HasForeignKey(x => x.RateId);
                b.HasOne<PostalOrg>().WithMany().HasForeignKey(x => x.PostalOrgId);
                b.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId);
                b.HasKey(x => x.Id);
            });

            builder.Entity<CustomerPostal>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "CustomerPostals");
                b.Property(x => x.Postal).HasColumnName(nameof(CustomerPostal.Postal));
                b.Property(x => x.Rate).HasColumnName(nameof(CustomerPostal.Rate));
                b.Property(x => x.AccountNo).HasColumnName(nameof(CustomerPostal.AccountNo));
                b.HasOne<Rate>().WithMany().HasForeignKey(x => x.Rate);
                b.HasOne<Customer>().WithMany().HasForeignKey(x => x.AccountNo);
                b.HasKey(x => x.Id);
            });

            builder.Entity<PostalCountry>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "PostalCountries");
                b.Property(x => x.PostalCode).HasColumnName(nameof(PostalCountry.PostalCode)).HasMaxLength(5);
                b.Property(x => x.CountryCode).HasColumnName(nameof(PostalCountry.CountryCode)).HasMaxLength(2);
                b.HasKey(x => x.Id);
            });

            builder.Entity<PostalOrg>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "PostalOrgs");
                b.Property(x => x.Name).HasColumnName(nameof(PostalOrg.Name)).HasMaxLength(30);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Postal>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Postals");
                b.Property(x => x.PostalCode).HasColumnName(nameof(Postal.PostalCode)).HasMaxLength(5);
                b.Property(x => x.PostalDesc).HasColumnName(nameof(Postal.PostalDesc)).HasMaxLength(30);
                b.Property(x => x.ServiceCode).HasColumnName(nameof(Postal.ServiceCode)).HasMaxLength(10);
                b.Property(x => x.ServiceDesc).HasColumnName(nameof(Postal.ServiceDesc)).HasMaxLength(30);
                b.Property(x => x.ProductCode).HasColumnName(nameof(Postal.ProductCode)).HasMaxLength(10);
                b.Property(x => x.ProductDesc).HasColumnName(nameof(Postal.ProductDesc)).HasMaxLength(30);
                b.Property(x => x.ItemTopUpValue).HasColumnName(nameof(Postal.ItemTopUpValue)).HasPrecision(18, 2);
                b.HasKey(x => x.Id);
            });

            builder.Entity<RateItem>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "RateItems");
                b.Property(x => x.RateId).HasColumnName(nameof(RateItem.RateId));
                b.Property(x => x.ServiceCode).HasColumnName(nameof(RateItem.ServiceCode)).HasMaxLength(2);
                b.Property(x => x.ProductCode).HasColumnName(nameof(RateItem.ProductCode)).HasMaxLength(10);
                b.Property(x => x.CountryCode).HasColumnName(nameof(RateItem.CountryCode)).HasMaxLength(2);
                b.Property(x => x.Total).HasColumnName(nameof(RateItem.Total)).HasPrecision(18, 2);
                b.Property(x => x.Fee).HasColumnName(nameof(RateItem.Fee)).HasPrecision(18, 2);
                b.Property(x => x.CurrencyId).HasColumnName(nameof(RateItem.CurrencyId));
                b.Property(x => x.PaymentMode).HasColumnName(nameof(RateItem.PaymentMode)).HasMaxLength(20);
                b.HasOne<Rate>().WithMany().HasForeignKey(x => x.RateId);
                b.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Rate>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Rates");
                b.Property(x => x.CardName).HasColumnName(nameof(Rate.CardName)).HasMaxLength(50);
                b.Property(x => x.Count).HasColumnName(nameof(Rate.Count));
                b.HasKey(x => x.Id);
            });

            builder.Entity<Wallet>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Wallets");
                b.Property(x => x.Customer).HasColumnName(nameof(Wallet.Customer));
                b.Property(x => x.EWalletType).HasColumnName(nameof(Wallet.EWalletType));
                b.Property(x => x.Currency).HasColumnName(nameof(Wallet.Currency));
                b.Property(x => x.Balance).HasColumnName(nameof(Wallet.Balance)).HasPrecision(18, 2);
                b.HasOne<Customer>().WithMany().HasForeignKey(x => x.Customer).HasPrincipalKey(x => x.Code);
                b.HasOne<EWalletType>().WithMany().HasForeignKey(x => x.EWalletType);
                b.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency);
                b.HasKey(x => new
                {
                    x.Customer,
                    x.EWalletType,
                    x.Currency,
                });
            });

            builder.Entity<Currency>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Currencies");
                b.Property(x => x.Abbr).HasColumnName(nameof(Currency.Abbr));
                b.Property(x => x.Description).HasColumnName(nameof(Currency.Description));
                b.HasKey(x => x.Id);
            });

            builder.Entity<EWalletType>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "EWalletTypes");
                b.Property(x => x.Type).HasColumnName(nameof(EWalletType.Type));
                b.HasKey(x => x.Id);
            });

            builder.Entity<Customer>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Customers");
                b.Property(x => x.CompanyName).HasColumnName(nameof(Customer.CompanyName));
                b.Property(x => x.EmailAddress).HasColumnName(nameof(Customer.EmailAddress));
                b.Property(x => x.Password).HasColumnName(nameof(Customer.Password));
                b.Property(x => x.AddressLine1).HasColumnName(nameof(Customer.AddressLine1));
                b.Property(x => x.AddressLine2).HasColumnName(nameof(Customer.AddressLine2));
                b.Property(x => x.City).HasColumnName(nameof(Customer.City));
                b.Property(x => x.State).HasColumnName(nameof(Customer.State));
                b.Property(x => x.Country).HasColumnName(nameof(Customer.Country));
                b.Property(x => x.PhoneNumber).HasColumnName(nameof(Customer.PhoneNumber));
                b.Property(x => x.RegistrationNo).HasColumnName(nameof(Customer.RegistrationNo));
                b.Property(x => x.EmailAddress2).HasColumnName(nameof(Customer.EmailAddress2));
                b.Property(x => x.EmailAddress3).HasColumnName(nameof(Customer.EmailAddress3));
                b.Property(x => x.IsActive).HasColumnName(nameof(Customer.IsActive));
                b.HasKey(x => x.Id);
                b.HasAlternateKey(x => x.Code);
            });

            builder.Entity<CustomerTransaction>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "CustomerTransactions");
                b.Property(x => x.Wallet).HasColumnName(nameof(CustomerTransaction.Wallet)).HasMaxLength(8);
                b.Property(x => x.Customer).HasColumnName(nameof(CustomerTransaction.Customer)).HasMaxLength(10);
                b.Property(x => x.PaymentMode).HasColumnName(nameof(CustomerTransaction.PaymentMode)).HasMaxLength(20);
                b.Property(x => x.Currency).HasColumnName(nameof(CustomerTransaction.Currency)).HasMaxLength(3);
                b.Property(x => x.TransactionType).HasColumnName(nameof(CustomerTransaction.TransactionType)).HasMaxLength(100);
                b.Property(x => x.Amount).HasColumnName(nameof(CustomerTransaction.Amount)).HasPrecision(18, 2);
                b.Property(x => x.ReferenceNo).HasColumnName(nameof(CustomerTransaction.ReferenceNo)).HasMaxLength(100);
                b.Property(x => x.Description).HasColumnName(nameof(CustomerTransaction.Description)).HasMaxLength(100);
                b.Property(x => x.TransactionDate).HasColumnName(nameof(CustomerTransaction.TransactionDate));
            });
        }
    }
}
