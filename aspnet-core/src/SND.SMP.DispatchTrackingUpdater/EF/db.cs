using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using SND.SMP.ApplicationSettings;
using SND.SMP.Chibis;
using SND.SMP.CustomerTransactions;
using SND.SMP.EWalletTypes;
using SND.SMP.Wallets;

namespace SND.SMP.DispatchTrackingUpdater.EF;

public partial class db : DbContext
{
    public db()
    {
    }

    public db(DbContextOptions<db> options)
        : base(options)
    {
    }

    public virtual DbSet<Bag> Bags { get; set; }

    public virtual DbSet<SND.SMP.Currencies.Currency> Currencies { get; set; }

    public virtual DbSet<SND.SMP.Customers.Customer> Customers { get; set; }

    public virtual DbSet<Customercurrency> Customercurrencies { get; set; }

    public virtual DbSet<SND.SMP.Wallets.Wallet> Wallets { get; set; }

    public virtual DbSet<SND.SMP.CustomerPostals.CustomerPostal> Customerpostals { get; set; }

    public virtual DbSet<Customertransaction> Customertransactions { get; set; }

    public virtual DbSet<Dispatch> Dispatches { get; set; }

    public virtual DbSet<Dispatchvalidation> Dispatchvalidations { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Itemmin> Itemmins { get; set; }

    public virtual DbSet<Postal> Postals { get; set; }

    public virtual DbSet<Postalcountry> Postalcountries { get; set; }

    public virtual DbSet<Postalorg> Postalorgs { get; set; }

    public virtual DbSet<Queue> Queues { get; set; }

    public virtual DbSet<Rate> Rates { get; set; }

    public virtual DbSet<Rateitem> Rateitems { get; set; }

    public virtual DbSet<Rateweightbreak> Rateweightbreaks { get; set; }

    public virtual DbSet<Chibi> Chibis { get; set; }

    public virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; }

    public virtual DbSet<EWalletType> EWalletTypes { get; set; }

    public virtual DbSet<CustomerTransaction> CustomerTransactions { get; set; }

    public virtual DbSet<SND.SMP.ItemTrackingApplications.ItemTrackingApplication> ItemTrackingApplications { get; set; }
    
    public virtual DbSet<SND.SMP.ItemTrackingReviews.ItemTrackingReview> ItemTrackingReviews { get; set; }

    public virtual DbSet<SND.SMP.ItemIdRunningNos.ItemIdRunningNo> ItemIdRunningNos { get; set; }

    public virtual DbSet<SND.SMP.DispatchUsedAmounts.DispatchUsedAmount> DispatchUsedAmounts { get; set; }
    
    public virtual DbSet<SND.SMP.TrackingNoForUpdates.TrackingNoForUpdate> TrackingNoForUpdates { get; set; }

    public virtual DbSet<SND.SMP.ItemTrackings.ItemTracking> ItemTrackings { get; set; }

    public virtual DbSet<SND.SMP.Airports.Airport> Airports { get; set; }

    public virtual DbSet<SND.SMP.WeightAdjustments.WeightAdjustment> WeightAdjustments { get; set; }

    public virtual DbSet<SND.SMP.Invoices.Invoice> Invoices { get; set; }

    public virtual DbSet<SND.SMP.RateZones.RateZone> RateZones { get; set; }



    // public virtual DbSet<SND.SMP.ApplicationSettings.ApplicationSetting> ApplicationSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=65.21.224.66;port=3306;database=SMPDb;uid=droot;pwd=snd@1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));
        // => optionsBuilder.UseMySql("server=signaturemail.co;port=3306;database=SMPDb;uid=smi_dev;pwd=C56bV{(}Igx<Pqx", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ApplicationSetting>(b =>
        {
            b.ToTable(SMPConsts.DbTablePrefix + "ApplicationSettings");
            b.Property(x => x.Name).HasColumnName(nameof(ApplicationSetting.Name)).HasMaxLength(64);
            b.Property(x => x.Value).HasColumnName(nameof(ApplicationSetting.Value)).HasMaxLength(256);
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Bag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bags");

            entity.HasIndex(e => e.DispatchId, "DispatchId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BagNo).HasMaxLength(20);
            entity.Property(e => e.Cn35no)
                .HasMaxLength(20)
                .HasColumnName("CN35No");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.UnderAmount).HasPrecision(18, 2);
            entity.Property(e => e.WeightPost).HasPrecision(18, 3);
            entity.Property(e => e.WeightPre).HasPrecision(18, 3);
            entity.Property(e => e.WeightVariance).HasPrecision(18, 3);

            entity.HasOne(d => d.Dispatch).WithMany(p => p.Bags)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bag_ibfk_1");
        });

        modelBuilder.Entity<Wallets.Wallet>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Wallets");
                b.Property(x => x.Customer).HasColumnName(nameof(Wallet.Customer));
                b.Property(x => x.EWalletType).HasColumnName(nameof(Wallet.EWalletType));
                b.Property(x => x.Currency).HasColumnName(nameof(Wallet.Currency));
                b.Property(x => x.Balance).HasColumnName(nameof(Wallet.Balance)).HasPrecision(18, 2);
                b.HasOne<Customer>().WithMany().HasForeignKey(x => x.Customer).HasPrincipalKey(x => x.Code);
                b.HasOne<SND.SMP.EWalletTypes.EWalletType>().WithMany().HasForeignKey(x => x.EWalletType);
                b.HasOne<SND.SMP.Currencies.Currency>().WithMany().HasForeignKey(x => x.Currency);
                b.HasKey(x => new
                {
                    x.Customer,
                    x.EWalletType,
                    x.Currency,
                });
            });

        // modelBuilder.Entity<Currency>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("PRIMARY");

        //     entity.ToTable("currency");

        //     entity.Property(e => e.Id)
        //         .HasMaxLength(3)
        //         .HasDefaultValueSql("''")
        //         .IsFixedLength()
        //         .HasColumnName("id");
        //     entity.Property(e => e.Name)
        //         .HasMaxLength(20)
        //         .HasColumnName("name");
        // });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("customers");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CompanyName).HasMaxLength(50);
        });

        // modelBuilder.Entity<SND.SMP.Customers.Customer>(b =>
        // {
        //     b.ToTable(SMPConsts.DbTablePrefix + "Customers");
        //     b.Property(x => x.CompanyName).HasColumnName(nameof(SND.SMP.Customers.Customer.CompanyName));
        //     b.Property(x => x.EmailAddress).HasColumnName(nameof(SND.SMP.Customers.Customer.EmailAddress));
        //     b.Property(x => x.Password).HasColumnName(nameof(SND.SMP.Customers.Customer.Password));
        //     b.Property(x => x.AddressLine1).HasColumnName(nameof(SND.SMP.Customers.Customer.AddressLine1));
        //     b.Property(x => x.AddressLine2).HasColumnName(nameof(SND.SMP.Customers.Customer.AddressLine2));
        //     b.Property(x => x.City).HasColumnName(nameof(SND.SMP.Customers.Customer.City));
        //     b.Property(x => x.State).HasColumnName(nameof(SND.SMP.Customers.Customer.State));
        //     b.Property(x => x.Country).HasColumnName(nameof(SND.SMP.Customers.Customer.Country));
        //     b.Property(x => x.PhoneNumber).HasColumnName(nameof(SND.SMP.Customers.Customer.PhoneNumber));
        //     b.Property(x => x.RegistrationNo).HasColumnName(nameof(SND.SMP.Customers.Customer.RegistrationNo));
        //     b.Property(x => x.EmailAddress2).HasColumnName(nameof(SND.SMP.Customers.Customer.EmailAddress2));
        //     b.Property(x => x.EmailAddress3).HasColumnName(nameof(SND.SMP.Customers.Customer.EmailAddress3));
        //     b.Property(x => x.IsActive).HasColumnName(nameof(SND.SMP.Customers.Customer.IsActive));
        //     b.HasKey(x => x.Id);
        //     b.HasAlternateKey(x => x.Code);
        // });

        modelBuilder.Entity<Customercurrency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("customercurrencys");

            entity.HasIndex(e => e.CurrencyId, "CurrencyId");

            entity.HasIndex(e => e.CustomerCode, "CustomerCode");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CurrencyId)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.CustomerCode).HasMaxLength(10);

            entity.HasOne(d => d.Currency).WithMany(p => p.Customercurrencies)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customercurrency_ibfk_2");

            entity.HasOne(d => d.CustomerCodeNavigation).WithMany(p => p.Customercurrencies)
                .HasForeignKey(d => d.CustomerCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customercurrency_ibfk_1");
        });

        modelBuilder.Entity<Customerpostal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("customerpostals");

            entity.HasIndex(e => e.AccountNo, "AccountNo");

            entity.HasIndex(e => e.Rate, "Rate");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNo).HasMaxLength(10);
            entity.Property(e => e.Postal).HasMaxLength(5);

            entity.HasOne(d => d.CustomerCodeNavigation).WithMany(p => p.Customerpostals)
                .HasForeignKey(d => d.AccountNo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customerpostal_ibfk_1");

            entity.HasOne(d => d.RateNav).WithMany(p => p.Customerpostals)
                .HasForeignKey(d => d.Rate)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customerpostal_ibfk_2");
        });

        modelBuilder.Entity<Customertransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("customertransactions");

            entity.HasIndex(e => e.CustomerCode, "customerCode");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CurrencyId)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currencyId");
            entity.Property(e => e.CustomerCode)
                .HasMaxLength(10)
                .HasColumnName("customerCode");
            entity.Property(e => e.DateTransaction)
                .HasColumnType("datetime")
                .HasColumnName("dateTransaction");
            entity.Property(e => e.Description)
                .HasMaxLength(80)
                .HasColumnName("description");
            entity.Property(e => e.RefNo)
                .HasMaxLength(20)
                .HasColumnName("refNo");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(10)
                .HasColumnName("transactionType");

            entity.HasOne(d => d.CustomerCodeNavigation).WithMany(p => p.Customertransactions)
                .HasForeignKey(d => d.CustomerCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customertransaction_ibfk_1");
        });

        modelBuilder.Entity<Dispatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispatches");

            entity.Property(e => e.AirlineCode).HasMaxLength(20);
            entity.Property(e => e.AirportTranshipment).HasMaxLength(20);
            entity.Property(e => e.AirwayBldate)
                .HasColumnType("datetime")
                .HasColumnName("AirwayBLDate");
            entity.Property(e => e.AirwayBlno)
                .HasMaxLength(20)
                .HasColumnName("AirwayBLNo");
            entity.Property(e => e.Ata)
                .HasColumnType("datetime")
                .HasColumnName("ATA");
            entity.Property(e => e.BatchId).HasMaxLength(35);
            entity.Property(e => e.Brcn38requestId)
                .HasMaxLength(20)
                .HasColumnName("BRCN38RequestId");
            entity.Property(e => e.Cn38)
                .HasMaxLength(30)
                .HasColumnName("CN38");
            entity.Property(e => e.CorateOptionId)
                .HasMaxLength(10)
                .HasColumnName("CORateOptionId");
            entity.Property(e => e.CountryOfLoading).HasMaxLength(50);
            entity.Property(e => e.CurrencyId)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.CustomerCode).HasMaxLength(10);
            entity.Property(e => e.DateAcceptanceScanning).HasColumnType("datetime");
            entity.Property(e => e.DateArrival).HasColumnType("datetime");
            entity.Property(e => e.DateClstage1Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage1Submitted");
            entity.Property(e => e.DateClstage2Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage2Submitted");
            entity.Property(e => e.DateClstage3Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage3Submitted");
            entity.Property(e => e.DateClstage4Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage4Submitted");
            entity.Property(e => e.DateClstage5Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage5Submitted");
            entity.Property(e => e.DateClstage6Submitted)
                .HasColumnType("datetime")
                .HasColumnName("DateCLStage6Submitted");
            entity.Property(e => e.DateEndedApi)
                .HasColumnType("datetime")
                .HasColumnName("DateEndedAPI");
            entity.Property(e => e.DateFlight).HasColumnType("datetime");
            entity.Property(e => e.DateFlightArrival).HasColumnType("datetime");
            entity.Property(e => e.DateLocalDelivery).HasColumnType("datetime");
            entity.Property(e => e.DatePerformanceDaysDiff).HasColumnType("datetime");
            entity.Property(e => e.DateSoaprocessCompleted)
                .HasColumnType("datetime")
                .HasColumnName("DateSOAProcessCompleted");
            entity.Property(e => e.DateStartedApi)
                .HasColumnType("datetime")
                .HasColumnName("DateStartedAPI");
            entity.Property(e => e.DispatchNo).HasMaxLength(15);
            entity.Property(e => e.EtatoHkg).HasColumnName("ETAtoHKG");
            entity.Property(e => e.ExtDispatchNo).HasMaxLength(50);
            entity.Property(e => e.FlightNo).HasMaxLength(20);
            entity.Property(e => e.FlightTrucking).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnType("bit(1)");
            entity.Property(e => e.IsPayment).HasColumnType("bit(1)");
            entity.Property(e => e.OfficeDestination)
                .HasMaxLength(30)
                .HasDefaultValueSql("NULL");
            entity.Property(e => e.OfficeOrigin).HasMaxLength(30);
            entity.Property(e => e.PaymentMode).HasMaxLength(10);
            entity.Property(e => e.Pobox)
                .HasMaxLength(10)
                .HasColumnName("POBox");
            entity.Property(e => e.PortDeparture).HasMaxLength(50);
            entity.Property(e => e.PostCheckTotalWeight).HasPrecision(18, 3);
            entity.Property(e => e.PostDeclarationDate).HasColumnType("datetime");
            entity.Property(e => e.PostDeclarationMsg)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PostDeclarationSuccess).HasColumnType("bit(1)");
            entity.Property(e => e.PostManifestDate).HasColumnType("datetime");
            entity.Property(e => e.PostManifestMsg)
                .HasMaxLength(150)
                .HasDefaultValueSql("NULL")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PostManifestSuccess).HasColumnType("bit(1)");
            entity.Property(e => e.PostalCode).HasMaxLength(5);
            entity.Property(e => e.Ppi)
                .HasMaxLength(10)
                .HasColumnName("PPI");
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.Remark)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ServiceCode).HasMaxLength(10);
            entity.Property(e => e.SoaprocessCompletedById).HasColumnName("SOAProcessCompletedByID");
            entity.Property(e => e.Stage1StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage2StatusDesc)
                .HasMaxLength(250)
                .HasDefaultValueSql("NULL")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage3StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage4StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage5StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage6StatusDesc)
                .HasMaxLength(250)
                .HasDefaultValueSql("NULL")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage7StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage8StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage9StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StatusApi)
                .HasMaxLength(250)
                .HasColumnName("StatusAPI");
            entity.Property(e => e.TotalAmountSoa)
                .HasPrecision(18, 2)
                .HasColumnName("TotalAmountSOA");
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalWeight).HasPrecision(18, 3);
            entity.Property(e => e.TotalWeightSoa)
                .HasPrecision(18, 3)
                .HasColumnName("TotalWeightSOA");
            entity.Property(e => e.TransactionDateTime).HasColumnType("datetime");
            entity.Property(e => e.WeightAveraged).HasPrecision(18, 3);
            entity.Property(e => e.WeightGap).HasPrecision(18, 3);
        });

        modelBuilder.Entity<Dispatchvalidation>(entity =>
        {
            entity.HasKey(e => e.DispatchNo).HasName("PRIMARY");

            entity.ToTable("dispatchvalidations");

            entity.Property(e => e.DispatchNo)
                .HasMaxLength(15)
                .HasDefaultValueSql("''");
            entity.Property(e => e.CustomerCode).HasMaxLength(10);
            entity.Property(e => e.DateCompleted).HasColumnType("datetime");
            entity.Property(e => e.DateStarted).HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(200);
            entity.Property(e => e.IsFundLack).HasColumnType("bit(1)");
            entity.Property(e => e.IsValid).HasColumnType("bit(1)");
            entity.Property(e => e.PostalCode).HasMaxLength(5);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.ServiceCode).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => new
                {
                    e.Id,
                    e.DispatchId,
                }).HasName("PRIMARY");

            entity.ToTable("items");

            entity.HasIndex(e => e.BagId, "BagID");

            entity.HasIndex(e => e.DispatchId, "DispatchID");

            entity.Property(e => e.Id).HasMaxLength(16);
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Address2)
                .HasMaxLength(300)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.AddressNo)
                .HasMaxLength(40)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.BagNo).HasMaxLength(20);
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CityId).HasMaxLength(50);
            entity.Property(e => e.ClabreviaturaCentro)
                .HasMaxLength(500)
                .HasColumnName("CLAbreviaturaCentro")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClabreviaturaServicio)
                .HasMaxLength(200)
                .HasColumnName("CLAbreviaturaServicio")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClcodigoDelegacionDestino)
                .HasMaxLength(200)
                .HasColumnName("CLCodigoDelegacionDestino")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClcodigoEncaminamiento)
                .HasMaxLength(200)
                .HasColumnName("CLCodigoEncaminamiento")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClcomunaDestino)
                .HasMaxLength(200)
                .HasColumnName("CLComunaDestino")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Clcuartel)
                .HasMaxLength(200)
                .HasColumnName("CLCuartel")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CldireccionDestino)
                .HasMaxLength(200)
                .HasColumnName("CLDireccionDestino")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClnombreDelegacionDestino)
                .HasMaxLength(200)
                .HasColumnName("CLNombreDelegacionDestino")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ClnumeroEnvio)
                .HasMaxLength(200)
                .HasColumnName("CLNumeroEnvio")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Clsdp)
                .HasMaxLength(200)
                .HasColumnName("CLSDP")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Clsector)
                .HasMaxLength(200)
                .HasColumnName("CLSector")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.DateStage1).HasColumnType("datetime");
            entity.Property(e => e.DateStage2).HasColumnType("datetime");
            entity.Property(e => e.DateStage3).HasColumnType("datetime");
            entity.Property(e => e.DateStage4).HasColumnType("datetime");
            entity.Property(e => e.DateStage5).HasColumnType("datetime");
            entity.Property(e => e.DateStage6).HasColumnType("datetime");
            entity.Property(e => e.DateStage7).HasColumnType("datetime");
            entity.Property(e => e.DateStage8).HasColumnType("datetime");
            entity.Property(e => e.DateStage9).HasColumnType("datetime");
            entity.Property(e => e.DateSuccessfulDelivery).HasColumnType("datetime");
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExemptedRemark).HasMaxLength(100);
            entity.Property(e => e.ExtId)
                .HasMaxLength(20)
                .HasColumnName("ExtID");
            entity.Property(e => e.ExtMsg)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.FinalOfficeId).HasMaxLength(50);
            entity.Property(e => e.Height).HasPrecision(18, 3);
            entity.Property(e => e.Hscode)
                .HasMaxLength(50)
                .HasColumnName("HSCode");
            entity.Property(e => e.IdentityType).HasMaxLength(20);
            entity.Property(e => e.Iosstax)
                .HasMaxLength(50)
                .HasColumnName("IOSSTax");
            entity.Property(e => e.IsDelivered).HasColumnType("bit(1)");
            entity.Property(e => e.IsExempted).HasColumnType("bit(1)");
            entity.Property(e => e.ItemDesc)
                .HasMaxLength(500)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ItemValue).HasPrecision(18, 2);
            entity.Property(e => e.Length).HasPrecision(18, 3);
            entity.Property(e => e.PassportNo).HasMaxLength(20);
            entity.Property(e => e.PostalCode).HasMaxLength(5);
            entity.Property(e => e.Postcode).HasMaxLength(20);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.RateCategory).HasMaxLength(10);
            entity.Property(e => e.RecpName)
                .HasMaxLength(300)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RefNo).HasMaxLength(100);
            entity.Property(e => e.SealNo).HasMaxLength(10);
            entity.Property(e => e.SenderName).HasMaxLength(128);
            entity.Property(e => e.ServiceCode).HasMaxLength(10);
            entity.Property(e => e.Stage1StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage2StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage3StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage4StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage5StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage6OmtarrivalDate)
                .HasColumnType("datetime")
                .HasColumnName("Stage6OMTArrivalDate");
            entity.Property(e => e.Stage6OmtcountryCode)
                .HasMaxLength(2)
                .HasColumnName("Stage6OMTCountryCode");
            entity.Property(e => e.Stage6OmtdepartureDate)
                .HasColumnType("datetime")
                .HasColumnName("Stage6OMTDepartureDate");
            entity.Property(e => e.Stage6OmtdestinationCity)
                .HasMaxLength(50)
                .HasColumnName("Stage6OMTDestinationCity");
            entity.Property(e => e.Stage6OmtdestinationCityCode)
                .HasMaxLength(20)
                .HasColumnName("Stage6OMTDestinationCityCode");
            entity.Property(e => e.Stage6OmtstatusDesc)
                .HasMaxLength(500)
                .HasColumnName("Stage6OMTStatusDesc")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage6StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage7StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage8StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stage9StatusDesc)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TaxPayMethod).HasMaxLength(20);
            entity.Property(e => e.TelNo).HasMaxLength(100);
            entity.Property(e => e.Weight).HasPrecision(18, 3);
            entity.Property(e => e.Width).HasPrecision(18, 3);

            entity.HasOne(d => d.Bag).WithMany(p => p.Items)
                .HasForeignKey(d => d.BagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("item_ibfk_2");

            entity.HasOne(d => d.Dispatch).WithMany(p => p.Items)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("item_ibfk_1");
        });

        modelBuilder.Entity<Itemmin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("itemmins");

            entity.HasIndex(e => e.BagId, "BagID");

            entity.HasIndex(e => e.DispatchId, "DispatchID");

            entity.Property(e => e.Id)
                .HasMaxLength(16)
                .HasDefaultValueSql("''")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .IsFixedLength();
            entity.Property(e => e.BagId).HasColumnName("BagID");
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsFixedLength();
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.ExtId)
                .HasMaxLength(20)
                .HasColumnName("ExtID");
            entity.Property(e => e.IsDelivered).HasColumnType("bit(1)");
            entity.Property(e => e.ItemDesc)
                .HasMaxLength(60)
                .IsFixedLength();
            entity.Property(e => e.ItemValue).HasPrecision(18, 2);
            entity.Property(e => e.RecpName)
                .HasMaxLength(30)
                .IsFixedLength();
            entity.Property(e => e.TelNo)
                .HasMaxLength(15)
                .IsFixedLength();
            entity.Property(e => e.Weight).HasPrecision(18, 2);

            entity.HasOne(d => d.Bag).WithMany(p => p.Itemmins)
                .HasForeignKey(d => d.BagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("itemmin_ibfk_2");

            entity.HasOne(d => d.Dispatch).WithMany(p => p.Itemmins)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("itemmin_ibfk_1");
        });

        modelBuilder.Entity<Postal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("postals");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemTopupValue)
                .HasPrecision(18, 2)
                .HasColumnName("itemTopupValue");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(5)
                .HasColumnName("postalCode");
            entity.Property(e => e.PostalDescription)
                .HasMaxLength(30)
                .HasColumnName("postalDescription");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(10)
                .HasColumnName("productCode");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(30)
                .HasColumnName("productDescription");
            entity.Property(e => e.ServiceCode)
                .HasMaxLength(10)
                .HasColumnName("serviceCode");
            entity.Property(e => e.ServiceDescription)
                .HasMaxLength(30)
                .HasColumnName("serviceDescription");
        });

        modelBuilder.Entity<Postalcountry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("postalcountries");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("countryCode");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(5)
                .HasColumnName("postalCode");
        });

        modelBuilder.Entity<Postalorg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("postalorgs");

            entity.Property(e => e.Id)
                .HasMaxLength(2)
                .HasDefaultValueSql("''")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Queue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("queues");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.DeleteFileOnFailed).HasColumnType("bit(1)");
            entity.Property(e => e.DeleteFileOnSuccess).HasColumnType("bit(1)");
            entity.Property(e => e.ErrorMsg).HasColumnType("text");
            entity.Property(e => e.EventType).HasMaxLength(20);
            entity.Property(e => e.FilePath).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CardName).HasMaxLength(30);
        });

        modelBuilder.Entity<Rateitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rateitems");

            entity.HasIndex(e => e.RateId, "RateId");

            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.CurrencyId)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.PaymentMode).HasMaxLength(20);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.Fee).HasPrecision(18, 2);
            entity.Property(e => e.ServiceCode).HasMaxLength(2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(d => d.Rate).WithMany(p => p.Rateitems)
                .HasForeignKey(d => d.RateId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("rateitem_ibfk_1");
        });

        modelBuilder.Entity<Rateweightbreak>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rateweightbreaks");

            entity.HasIndex(e => e.PostalOrgId, "PostalId");

            entity.HasIndex(e => e.RateId, "RateId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyId)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.IsExceedRule).HasColumnType("bit(1)");
            entity.Property(e => e.ItemRate).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMode).HasMaxLength(20);
            entity.Property(e => e.PostalOrgId).HasMaxLength(2);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.WeightMax).HasPrecision(18, 3);
            entity.Property(e => e.WeightMin).HasPrecision(18, 3);
            entity.Property(e => e.WeightRate).HasPrecision(18, 2);

            entity.HasOne(d => d.PostalOrg).WithMany(p => p.Rateweightbreaks)
                .HasForeignKey(d => d.PostalOrgId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("rateweightbreak_ibfk_2");

            entity.HasOne(d => d.Rate).WithMany(p => p.Rateweightbreaks)
                .HasForeignKey(d => d.RateId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("rateweightbreak_ibfk_1");
        });

        modelBuilder.Entity<Chibi>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Chibis");
                b.Property(x => x.FileName).HasColumnName(nameof(Chibi.FileName)).HasMaxLength(128);
                b.Property(x => x.UUID).HasColumnName(nameof(Chibi.UUID)).HasMaxLength(128);
                b.Property(x => x.URL).HasColumnName(nameof(Chibi.URL)).HasMaxLength(128);
                b.Property(x => x.OriginalName).HasColumnName(nameof(Chibi.OriginalName)).HasMaxLength(128);
                b.Property(x => x.GeneratedName).HasColumnName(nameof(Chibi.GeneratedName)).HasMaxLength(128);
                b.HasKey(x => x.Id);
            });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
