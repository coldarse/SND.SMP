using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using SND.SMP.Authorization.Roles;
using SND.SMP.Authorization.Users;
using SND.SMP.MultiTenancy;
/* Using Definition */
using SND.SMP.RateItems;
using SND.SMP.Rates;
using SND.SMP.Wallets;
using SND.SMP.Currencies;
using SND.SMP.EWalletTypes;
using SND.SMP.Customers;


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
        /* Define a DbSet for each entity of the application */
        
        public SMPDbContext(DbContextOptions<SMPDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* Define Tables */
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

        }
    }
}
