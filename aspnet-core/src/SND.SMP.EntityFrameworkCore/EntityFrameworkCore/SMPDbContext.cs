using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using SND.SMP.Authorization.Roles;
using SND.SMP.Authorization.Users;
using SND.SMP.MultiTenancy;
/* Using Definition */
using SND.SMP.ItemTrackingEvents;
using SND.SMP.SAParams;
using SND.SMP.RateZones;
using SND.SMP.Invoices;
using SND.SMP.TrackingStatuses;
using SND.SMP.TrackingNoForUpdates;
using SND.SMP.APIRequestResponses;
using SND.SMP.EmailContents;
using SND.SMP.DispatchUsedAmounts;
using SND.SMP.Airports;
using SND.SMP.ItemTrackings;
using SND.SMP.ItemIdRunningNos;
using SND.SMP.ItemTrackingReviews;
using SND.SMP.ItemTrackingApplications;
using SND.SMP.IMPCS;
using SND.SMP.Refunds;
using SND.SMP.WeightAdjustments;
using SND.SMP.ApplicationSettings;
using SND.SMP.ItemMins;
using SND.SMP.Items;
using SND.SMP.Bags;
using SND.SMP.DispatchValidations;
using SND.SMP.Dispatches;
using SND.SMP.Queues;
using SND.SMP.Chibis;
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
        public DbSet<Chibi> Chibis { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<Dispatch> Dispatches { get; set; }
        public DbSet<DispatchValidation> DispatchValidations { get; set; }
        public DbSet<Bag> Bags { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemMin> ItemMins { get; set; }
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
        public DbSet<WeightAdjustment> WeightAdjustments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<IMPC> IMPCS { get; set; }
        public DbSet<ItemTrackingApplication> ItemTrackingApplications { get; set; }
        public DbSet<ItemTrackingReview> ItemTrackingReviews { get; set; }
        public DbSet<ItemIdRunningNo> ItemIdRunningNos { get; set; }
        public DbSet<ItemTracking> ItemTrackings { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<DispatchUsedAmount> DispatchUsedAmounts { get; set; }
        public DbSet<EmailContent> EmailContents { get; set; }
        public DbSet<APIRequestResponse> APIRequestResponses { get; set; }
        public DbSet<TrackingNoForUpdate> TrackingNoForUpdates { get; set; }
        public DbSet<TrackingStatus> TrackingStatuses { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<RateZone> RateZones { get; set; }
        public DbSet<SAParam> SAParams { get; set; }
        public DbSet<ItemTrackingEvent> ItemTrackingEvents { get; set; }
        /* Define a DbSet for each entity of the application */

        public SMPDbContext(DbContextOptions<SMPDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* Define Tables */
            builder.Entity<ItemTrackingEvent>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemTrackingEvents");
                b.Property(x => x.TrackingNo).HasColumnName(nameof(ItemTrackingEvent.TrackingNo)).HasMaxLength(64);
                b.Property(x => x.Event).HasColumnName(nameof(ItemTrackingEvent.Event));
                b.Property(x => x.Status).HasColumnName(nameof(ItemTrackingEvent.Status)).HasMaxLength(512);
                b.Property(x => x.Country).HasColumnName(nameof(ItemTrackingEvent.Country)).HasMaxLength(4);
                b.Property(x => x.EventTime).HasColumnName(nameof(ItemTrackingEvent.EventTime));
                b.Property(x => x.DispatchNo).HasColumnName(nameof(ItemTrackingEvent.DispatchNo)).HasMaxLength(64);
                b.HasKey(x => x.Id);
            });

            builder.Entity<SAParam>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "SAParams");
                b.Property(x => x.Type).HasColumnName(nameof(SAParam.Type)).HasMaxLength(300);
                b.Property(x => x.Name).HasColumnName(nameof(SAParam.Name)).HasMaxLength(300);
                b.Property(x => x.Code).HasColumnName(nameof(SAParam.Code)).HasMaxLength(300);
                b.Property(x => x.PostOfficeId).HasColumnName(nameof(SAParam.PostOfficeId)).HasMaxLength(128);
                b.Property(x => x.CityId).HasColumnName(nameof(SAParam.CityId)).HasMaxLength(128);
                b.Property(x => x.FinalOfficeId).HasColumnName(nameof(SAParam.FinalOfficeId)).HasMaxLength(128);
                b.Property(x => x.Seq).HasColumnName(nameof(SAParam.Seq));
                b.HasKey(x => x.Id);
            });

            builder.Entity<RateZone>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "RateZones");
                b.Property(x => x.Zone).HasColumnName(nameof(RateZone.Zone)).HasMaxLength(64);
                b.Property(x => x.State).HasColumnName(nameof(RateZone.State)).HasMaxLength(64);
                b.Property(x => x.City).HasColumnName(nameof(RateZone.City)).HasMaxLength(64);
                b.Property(x => x.PostCode).HasColumnName(nameof(RateZone.PostCode)).HasMaxLength(24);
                b.Property(x => x.Country).HasColumnName(nameof(RateZone.Country)).HasMaxLength(64);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Invoice>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Invoices");
                b.Property(x => x.DateTime).HasColumnName(nameof(Invoice.DateTime));
                b.Property(x => x.InvoiceNo).HasColumnName(nameof(Invoice.InvoiceNo)).HasMaxLength(128);
                b.Property(x => x.Customer).HasColumnName(nameof(Invoice.Customer)).HasMaxLength(512);
                b.HasKey(x => x.Id);
            });

            builder.Entity<TrackingStatus>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "TrackingStatuses");
                b.Property(x => x.TrackingNo).HasColumnName(nameof(TrackingStatus.TrackingNo)).HasMaxLength(256);
                b.Property(x => x.DispatchId).HasColumnName(nameof(TrackingStatus.DispatchId));
                b.Property(x => x.DispatchNo).HasColumnName(nameof(TrackingStatus.DispatchNo)).HasMaxLength(128);
                b.Property(x => x.Status).HasColumnName(nameof(TrackingStatus.Status));
                b.Property(x => x.DateTime).HasColumnName(nameof(TrackingStatus.DateTime));
                b.HasKey(x => x.Id);
            });

            builder.Entity<TrackingNoForUpdate>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "TrackingNoForUpdates");
                b.Property(x => x.TrackingNo).HasColumnName(nameof(TrackingNoForUpdate.TrackingNo)).HasMaxLength(20);
                b.Property(x => x.DispatchNo).HasColumnName(nameof(TrackingNoForUpdate.DispatchNo)).HasMaxLength(40);
                b.Property(x => x.ProcessType).HasColumnName(nameof(TrackingNoForUpdate.ProcessType)).HasMaxLength(20);
                b.HasKey(x => x.Id);
            });

            builder.Entity<APIRequestResponse>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "APIRequestResponses");
                b.Property(x => x.URL).HasColumnName(nameof(APIRequestResponse.URL)).HasMaxLength(255);
                b.Property(x => x.RequestBody).HasColumnName(nameof(APIRequestResponse.RequestBody));
                b.Property(x => x.ResponseBody).HasColumnName(nameof(APIRequestResponse.ResponseBody));
                b.Property(x => x.RequestDateTime).HasColumnName(nameof(APIRequestResponse.RequestDateTime));
                b.Property(x => x.ResponseDateTime).HasColumnName(nameof(APIRequestResponse.ResponseDateTime));
                b.Property(x => x.Duration).HasColumnName(nameof(APIRequestResponse.Duration));
                b.HasKey(x => x.Id);
            });

            builder.Entity<EmailContent>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "EmailContents");
                b.Property(x => x.Name).HasColumnName(nameof(EmailContent.Name)).HasMaxLength(128);
                b.Property(x => x.Subject).HasColumnName(nameof(EmailContent.Subject)).HasMaxLength(128);
                b.Property(x => x.Content).HasColumnName(nameof(EmailContent.Content));
                b.HasKey(x => x.Id);
            });

            builder.Entity<DispatchUsedAmount>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "DispatchUsedAmounts");
                b.Property(x => x.CustomerCode).HasColumnName(nameof(DispatchUsedAmount.CustomerCode)).HasMaxLength(255);
                b.Property(x => x.Wallet).HasColumnName(nameof(DispatchUsedAmount.Wallet)).HasMaxLength(128);
                b.Property(x => x.Amount).HasColumnName(nameof(DispatchUsedAmount.Amount)).HasPrecision(18, 2);
                b.Property(x => x.DispatchNo).HasColumnName(nameof(DispatchUsedAmount.DispatchNo)).HasMaxLength(128);
                b.Property(x => x.DateTime).HasColumnName(nameof(DispatchUsedAmount.DateTime));
                b.Property(x => x.Description).HasColumnName(nameof(DispatchUsedAmount.Description)).HasMaxLength(512);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Airport>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Airports");
                b.Property(x => x.Name).HasColumnName(nameof(Airport.Name)).HasMaxLength(256);
                b.Property(x => x.Code).HasColumnName(nameof(Airport.Code)).HasMaxLength(32);
                b.Property(x => x.Country).HasColumnName(nameof(Airport.Country)).HasMaxLength(2);
                b.HasKey(x => x.Id);
            });

            builder.Entity<ItemTracking>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemTrackings");
                b.Property(x => x.TrackingNo).HasColumnName(nameof(ItemTracking.TrackingNo)).HasMaxLength(128);
                b.Property(x => x.ApplicationId).HasColumnName(nameof(ItemTracking.ApplicationId));
                b.Property(x => x.ReviewId).HasColumnName(nameof(ItemTracking.ReviewId));
                b.Property(x => x.CustomerId).HasColumnName(nameof(ItemTracking.CustomerId));
                b.Property(x => x.CustomerCode).HasColumnName(nameof(ItemTracking.CustomerCode)).HasMaxLength(128);
                b.Property(x => x.DateCreated).HasColumnName(nameof(ItemTracking.DateCreated));
                b.Property(x => x.DateUsed).HasColumnName(nameof(ItemTracking.DateUsed));
                b.Property(x => x.DispatchId).HasColumnName(nameof(ItemTracking.DispatchId));
                b.Property(x => x.DispatchNo).HasColumnName(nameof(ItemTracking.DispatchNo)).HasMaxLength(128);
                b.Property(x => x.ProductCode).HasColumnName(nameof(ItemTracking.ProductCode)).HasMaxLength(128);
                b.Property(x => x.IsExternal).HasColumnName(nameof(ItemTracking.IsExternal)).HasDefaultValue(false);
                b.HasKey(x => x.Id);
            });

            builder.Entity<ItemIdRunningNo>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemIdRunningNos");
                b.Property(x => x.Prefix).HasColumnName(nameof(ItemIdRunningNo.Prefix)).HasMaxLength(3);
                b.Property(x => x.PrefixNo).HasColumnName(nameof(ItemIdRunningNo.PrefixNo)).HasMaxLength(7);
                b.Property(x => x.Suffix).HasColumnName(nameof(ItemIdRunningNo.Suffix)).HasMaxLength(2);
                b.Property(x => x.RunningNo).HasColumnName(nameof(ItemIdRunningNo.RunningNo));
                b.Property(x => x.Customer).HasColumnName(nameof(ItemIdRunningNo.Customer)).HasMaxLength(128);
                b.HasKey(x => x.Id);
            });

            builder.Entity<ItemTrackingReview>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemTrackingReviews");
                b.Property(x => x.ApplicationId).HasColumnName(nameof(ItemTrackingReview.ApplicationId));
                b.Property(x => x.CustomerId).HasColumnName(nameof(ItemTrackingReview.CustomerId));
                b.Property(x => x.CustomerCode).HasColumnName(nameof(ItemTrackingReview.CustomerCode)).HasMaxLength(255);
                b.Property(x => x.Total).HasColumnName(nameof(ItemTrackingReview.Total));
                b.Property(x => x.PostalCode).HasColumnName(nameof(ItemTrackingReview.PostalCode)).HasMaxLength(2);
                b.Property(x => x.PostalDesc).HasColumnName(nameof(ItemTrackingReview.PostalDesc)).HasMaxLength(255);
                b.Property(x => x.DateCreated).HasColumnName(nameof(ItemTrackingReview.DateCreated));
                b.Property(x => x.Status).HasColumnName(nameof(ItemTrackingReview.Status)).HasMaxLength(128);
                b.Property(x => x.TotalGiven).HasColumnName(nameof(ItemTrackingReview.TotalGiven));
                b.Property(x => x.Prefix).HasColumnName(nameof(ItemTrackingReview.Prefix)).HasMaxLength(2);
                b.Property(x => x.PrefixNo).HasColumnName(nameof(ItemTrackingReview.PrefixNo)).HasMaxLength(4);
                b.Property(x => x.Suffix).HasColumnName(nameof(ItemTrackingReview.Suffix)).HasMaxLength(2);
                b.Property(x => x.ProductCode).HasColumnName(nameof(ItemTrackingReview.ProductCode)).HasMaxLength(128);
                b.Property(x => x.Remark).HasColumnName(nameof(ItemTrackingReview.Remark)).HasMaxLength(255);
                b.HasKey(x => x.Id);
            });

            builder.Entity<ItemTrackingApplication>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemTrackingApplications");
                b.Property(x => x.CustomerId).HasColumnName(nameof(ItemTrackingApplication.CustomerId));
                b.Property(x => x.CustomerCode).HasColumnName(nameof(ItemTrackingApplication.CustomerCode)).HasMaxLength(255);
                b.Property(x => x.PostalCode).HasColumnName(nameof(ItemTrackingApplication.PostalCode)).HasMaxLength(2);
                b.Property(x => x.PostalDesc).HasColumnName(nameof(ItemTrackingApplication.PostalDesc)).HasMaxLength(255);
                b.Property(x => x.Total).HasColumnName(nameof(ItemTrackingApplication.Total));
                b.Property(x => x.ProductCode).HasColumnName(nameof(ItemTrackingApplication.ProductCode)).HasMaxLength(128);
                b.Property(x => x.ProductDesc).HasColumnName(nameof(ItemTrackingApplication.ProductDesc)).HasMaxLength(255);
                b.Property(x => x.Status).HasColumnName(nameof(ItemTrackingApplication.Status)).HasMaxLength(128);
                b.Property(x => x.DateCreated).HasColumnName(nameof(ItemTrackingApplication.DateCreated));
                b.Property(x => x.Range).HasColumnName(nameof(ItemTrackingApplication.Range)).HasMaxLength(255);
                b.Property(x => x.Path).HasColumnName(nameof(ItemTrackingApplication.Path)).HasMaxLength(255);
                b.Property(x => x.TookInSec).HasColumnName(nameof(ItemTrackingApplication.TookInSec));
                b.HasKey(x => x.Id);
            });

            builder.Entity<IMPC>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "IMPCS");
                b.Property(x => x.Type).HasColumnName(nameof(IMPC.Type)).HasMaxLength(100);
                b.Property(x => x.CountryCode).HasColumnName(nameof(IMPC.CountryCode)).HasMaxLength(10);
                b.Property(x => x.AirportCode).HasColumnName(nameof(IMPC.AirportCode)).HasMaxLength(20);
                b.Property(x => x.IMPCCode).HasColumnName(nameof(IMPC.IMPCCode)).HasMaxLength(20);
                b.Property(x => x.LogisticCode).HasColumnName(nameof(IMPC.LogisticCode)).HasMaxLength(20);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Refund>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Refunds");
                b.Property(x => x.UserId).HasColumnName(nameof(Refund.UserId));
                b.Property(x => x.ReferenceNo).HasColumnName(nameof(Refund.ReferenceNo)).HasMaxLength(128);
                b.Property(x => x.Amount).HasColumnName(nameof(Refund.Amount)).HasPrecision(18, 2);
                b.Property(x => x.Description).HasColumnName(nameof(Refund.Description)).HasMaxLength(512);
                b.Property(x => x.DateTime).HasColumnName(nameof(Refund.DateTime));
                b.Property(x => x.Weight).HasColumnName(nameof(Refund.Weight)).HasPrecision(18, 3);
                b.HasKey(x => x.Id);
            });

            builder.Entity<WeightAdjustment>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "WeightAdjustments");
                b.Property(x => x.UserId).HasColumnName(nameof(WeightAdjustment.UserId));
                b.Property(x => x.ReferenceNo).HasColumnName(nameof(WeightAdjustment.ReferenceNo)).HasMaxLength(128);
                b.Property(x => x.Amount).HasColumnName(nameof(WeightAdjustment.Amount)).HasPrecision(18, 2);
                b.Property(x => x.Description).HasColumnName(nameof(WeightAdjustment.Description)).HasMaxLength(512);
                b.Property(x => x.DateTime).HasColumnName(nameof(WeightAdjustment.DateTime));
                b.Property(x => x.Weight).HasColumnName(nameof(WeightAdjustment.Weight)).HasPrecision(18, 3);
                b.Property(x => x.InvoiceId).HasColumnName(nameof(WeightAdjustment.InvoiceId));
                b.HasKey(x => x.Id);
            });

            builder.Entity<ApplicationSetting>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ApplicationSettings");
                b.Property(x => x.Name).HasColumnName(nameof(ApplicationSetting.Name)).HasMaxLength(64);
                b.Property(x => x.Value).HasColumnName(nameof(ApplicationSetting.Value)).HasMaxLength(256);
                b.HasKey(x => x.Id);
            });

            builder.Entity<ItemMin>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "ItemMins");
                b.Property(x => x.ExtID                         ).HasColumnName(nameof(ItemMin.ExtID                         )).HasMaxLength(20);
                b.Property(x => x.DispatchID                    ).HasColumnName(nameof(ItemMin.DispatchID                    ));
                b.Property(x => x.BagID                         ).HasColumnName(nameof(ItemMin.BagID                         ));
                b.Property(x => x.DispatchDate                  ).HasColumnName(nameof(ItemMin.DispatchDate                  ));
                b.Property(x => x.Month                         ).HasColumnName(nameof(ItemMin.Month                         ));
                b.Property(x => x.CountryCode                   ).HasColumnName(nameof(ItemMin.CountryCode                   )).HasMaxLength(2);
                b.Property(x => x.Weight                        ).HasColumnName(nameof(ItemMin.Weight                        )).HasPrecision(18, 3);
                b.Property(x => x.ItemValue                     ).HasColumnName(nameof(ItemMin.ItemValue                     )).HasPrecision(18, 2);
                b.Property(x => x.RecpName                      ).HasColumnName(nameof(ItemMin.RecpName                      )).HasMaxLength(30);
                b.Property(x => x.ItemDesc                      ).HasColumnName(nameof(ItemMin.ItemDesc                      )).HasMaxLength(60);
                b.Property(x => x.Address                       ).HasColumnName(nameof(ItemMin.Address                       )).HasMaxLength(100);
                b.Property(x => x.City                          ).HasColumnName(nameof(ItemMin.City                          )).HasMaxLength(30);
                b.Property(x => x.TelNo                         ).HasColumnName(nameof(ItemMin.TelNo                         )).HasMaxLength(15);
                b.Property(x => x.DeliveredInDays               ).HasColumnName(nameof(ItemMin.DeliveredInDays               ));
                b.Property(x => x.IsDelivered                   ).HasColumnName(nameof(ItemMin.IsDelivered                   ));
                b.Property(x => x.Status                        ).HasColumnName(nameof(ItemMin.Status                        ));
                b.HasOne<Dispatch>().WithMany().HasForeignKey(x => x.DispatchID);
                b.HasOne<Bag>().WithMany().HasForeignKey(x => x.BagID);
                b.HasKey(x => x.Id);
                b.HasKey(x => new
                {
                    x.Id,
                    x.DispatchID,
                });
            });

            builder.Entity<Item>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Items");
                b.Property(x => x.ExtID                         ).HasColumnName(nameof(Item.ExtID                         )).HasMaxLength(20);
                b.Property(x => x.DispatchID                    ).HasColumnName(nameof(Item.DispatchID                    ));
                b.Property(x => x.BagID                         ).HasColumnName(nameof(Item.BagID                         ));
                b.Property(x => x.DispatchDate                  ).HasColumnName(nameof(Item.DispatchDate                  ));
                b.Property(x => x.Month                         ).HasColumnName(nameof(Item.Month                         ));
                b.Property(x => x.PostalCode                    ).HasColumnName(nameof(Item.PostalCode                    )).HasMaxLength(5);
                b.Property(x => x.ServiceCode                   ).HasColumnName(nameof(Item.ServiceCode                   )).HasMaxLength(10);
                b.Property(x => x.ProductCode                   ).HasColumnName(nameof(Item.ProductCode                   )).HasMaxLength(128);
                b.Property(x => x.CountryCode                   ).HasColumnName(nameof(Item.CountryCode                   )).HasMaxLength(2);
                b.Property(x => x.Weight                        ).HasColumnName(nameof(Item.Weight                        )).HasPrecision(18, 3);
                b.Property(x => x.BagNo                         ).HasColumnName(nameof(Item.BagNo                         )).HasMaxLength(20);
                b.Property(x => x.SealNo                        ).HasColumnName(nameof(Item.SealNo                        )).HasMaxLength(10);
                b.Property(x => x.Price                         ).HasColumnName(nameof(Item.Price                         )).HasPrecision(18, 2);
                b.Property(x => x.Status                        ).HasColumnName(nameof(Item.Status                        ));
                b.Property(x => x.ItemValue                     ).HasColumnName(nameof(Item.ItemValue                     )).HasPrecision(18, 2);
                b.Property(x => x.ItemDesc                      ).HasColumnName(nameof(Item.ItemDesc                      )).HasMaxLength(500);
                b.Property(x => x.RecpName                      ).HasColumnName(nameof(Item.RecpName                      )).HasMaxLength(300);
                b.Property(x => x.TelNo                         ).HasColumnName(nameof(Item.TelNo                         )).HasMaxLength(100);
                b.Property(x => x.Email                         ).HasColumnName(nameof(Item.Email                         )).HasMaxLength(100);
                b.Property(x => x.Address                       ).HasColumnName(nameof(Item.Address                       )).HasMaxLength(500);
                b.Property(x => x.Postcode                      ).HasColumnName(nameof(Item.Postcode                      )).HasMaxLength(20);
                b.Property(x => x.RateCategory                  ).HasColumnName(nameof(Item.RateCategory                  )).HasMaxLength(10);
                b.Property(x => x.City                          ).HasColumnName(nameof(Item.City                          )).HasMaxLength(100);
                b.Property(x => x.Address2                      ).HasColumnName(nameof(Item.Address2                      )).HasMaxLength(300);
                b.Property(x => x.AddressNo                     ).HasColumnName(nameof(Item.AddressNo                     )).HasMaxLength(300);
                b.Property(x => x.State                         ).HasColumnName(nameof(Item.State                         )).HasMaxLength(100);
                b.Property(x => x.Length                        ).HasColumnName(nameof(Item.Length                        )).HasPrecision(18, 3);
                b.Property(x => x.Width                         ).HasColumnName(nameof(Item.Width                         )).HasPrecision(18, 3);
                b.Property(x => x.Height                        ).HasColumnName(nameof(Item.Height                        )).HasPrecision(18, 3);
                b.Property(x => x.HSCode                        ).HasColumnName(nameof(Item.HSCode                        )).HasMaxLength(50);
                b.Property(x => x.Qty                           ).HasColumnName(nameof(Item.Qty                           ));
                b.Property(x => x.PassportNo                    ).HasColumnName(nameof(Item.PassportNo                    )).HasMaxLength(20);
                b.Property(x => x.TaxPayMethod                  ).HasColumnName(nameof(Item.TaxPayMethod                  )).HasMaxLength(20);
                b.Property(x => x.DateStage1                    ).HasColumnName(nameof(Item.DateStage1                    ));
                b.Property(x => x.DateStage2                    ).HasColumnName(nameof(Item.DateStage2                    ));
                b.Property(x => x.DateStage3                    ).HasColumnName(nameof(Item.DateStage3                    ));
                b.Property(x => x.DateStage4                    ).HasColumnName(nameof(Item.DateStage4                    ));
                b.Property(x => x.DateStage5                    ).HasColumnName(nameof(Item.DateStage5                    ));
                b.Property(x => x.DateStage6                    ).HasColumnName(nameof(Item.DateStage6                    ));
                b.Property(x => x.DateStage7                    ).HasColumnName(nameof(Item.DateStage7                    ));
                b.Property(x => x.DateStage8                    ).HasColumnName(nameof(Item.DateStage8                    ));
                b.Property(x => x.DateStage9                    ).HasColumnName(nameof(Item.DateStage9                    ));
                b.Property(x => x.Stage6OMTStatusDesc           ).HasColumnName(nameof(Item.Stage6OMTStatusDesc           )).HasMaxLength(500);
                b.Property(x => x.Stage6OMTDepartureDate        ).HasColumnName(nameof(Item.Stage6OMTDepartureDate        ));
                b.Property(x => x.Stage6OMTArrivalDate          ).HasColumnName(nameof(Item.Stage6OMTArrivalDate          ));
                b.Property(x => x.Stage6OMTDestinationCity      ).HasColumnName(nameof(Item.Stage6OMTDestinationCity      )).HasMaxLength(50);
                b.Property(x => x.Stage6OMTDestinationCityCode  ).HasColumnName(nameof(Item.Stage6OMTDestinationCityCode  )).HasMaxLength(20);
                b.Property(x => x.Stage6OMTCountryCode          ).HasColumnName(nameof(Item.Stage6OMTCountryCode          )).HasMaxLength(2);
                b.Property(x => x.ExtMsg                        ).HasColumnName(nameof(Item.ExtMsg                        )).HasMaxLength(150);
                b.Property(x => x.IdentityType                  ).HasColumnName(nameof(Item.IdentityType                  )).HasMaxLength(20);
                b.Property(x => x.SenderName                    ).HasColumnName(nameof(Item.SenderName                    )).HasMaxLength(128);
                b.Property(x => x.IOSSTax                       ).HasColumnName(nameof(Item.IOSSTax                       )).HasMaxLength(50);
                b.Property(x => x.RefNo                         ).HasColumnName(nameof(Item.RefNo                         )).HasMaxLength(100);
                b.Property(x => x.DateSuccessfulDelivery        ).HasColumnName(nameof(Item.DateSuccessfulDelivery        ));
                b.Property(x => x.IsDelivered                   ).HasColumnName(nameof(Item.IsDelivered                   ));
                b.Property(x => x.DeliveredInDays               ).HasColumnName(nameof(Item.DeliveredInDays               ));
                b.Property(x => x.IsExempted                    ).HasColumnName(nameof(Item.IsExempted                    ));
                b.Property(x => x.ExemptedRemark                ).HasColumnName(nameof(Item.ExemptedRemark                )).HasMaxLength(100);
                b.Property(x => x.CLCuartel                     ).HasColumnName(nameof(Item.CLCuartel                     )).HasMaxLength(200);
                b.Property(x => x.CLSector                      ).HasColumnName(nameof(Item.CLSector                      )).HasMaxLength(200);
                b.Property(x => x.CLSDP                         ).HasColumnName(nameof(Item.CLSDP                         )).HasMaxLength(200);
                b.Property(x => x.CLCodigoDelegacionDestino     ).HasColumnName(nameof(Item.CLCodigoDelegacionDestino     )).HasMaxLength(200);
                b.Property(x => x.CLNombreDelegacionDestino     ).HasColumnName(nameof(Item.CLNombreDelegacionDestino     )).HasMaxLength(200);
                b.Property(x => x.CLDireccionDestino            ).HasColumnName(nameof(Item.CLDireccionDestino            )).HasMaxLength(200);
                b.Property(x => x.CLCodigoEncaminamiento        ).HasColumnName(nameof(Item.CLCodigoEncaminamiento        )).HasMaxLength(200);
                b.Property(x => x.CLNumeroEnvio                 ).HasColumnName(nameof(Item.CLNumeroEnvio                 )).HasMaxLength(200);
                b.Property(x => x.CLComunaDestino               ).HasColumnName(nameof(Item.CLComunaDestino               )).HasMaxLength(200);
                b.Property(x => x.CLAbreviaturaServicio         ).HasColumnName(nameof(Item.CLAbreviaturaServicio         )).HasMaxLength(200);
                b.Property(x => x.CLAbreviaturaCentro           ).HasColumnName(nameof(Item.CLAbreviaturaCentro           )).HasMaxLength(500);
                b.Property(x => x.Stage1StatusDesc              ).HasColumnName(nameof(Item.Stage1StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage2StatusDesc              ).HasColumnName(nameof(Item.Stage2StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage3StatusDesc              ).HasColumnName(nameof(Item.Stage3StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage4StatusDesc              ).HasColumnName(nameof(Item.Stage4StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage5StatusDesc              ).HasColumnName(nameof(Item.Stage5StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage6StatusDesc              ).HasColumnName(nameof(Item.Stage6StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage7StatusDesc              ).HasColumnName(nameof(Item.Stage7StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage8StatusDesc              ).HasColumnName(nameof(Item.Stage8StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.Stage9StatusDesc              ).HasColumnName(nameof(Item.Stage9StatusDesc              )).HasMaxLength(250);
                b.Property(x => x.CityId                        ).HasColumnName(nameof(Item.CityId                        )).HasMaxLength(50);
                b.Property(x => x.FinalOfficeId                 ).HasColumnName(nameof(Item.FinalOfficeId                 )).HasMaxLength(50);
                b.HasOne<Dispatch>().WithMany().HasForeignKey(x => x.DispatchID);
                b.HasOne<Bag>().WithMany().HasForeignKey(x => x.BagID);
                b.HasKey(x => new
                {
                    x.Id,
                    x.DispatchID,
                });
            });

            builder.Entity<Bag>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Bags");
                b.Property(x => x.BagNo                         ).HasColumnName(nameof(Bag.BagNo                         )).HasMaxLength(20);
                b.Property(x => x.DispatchId                    ).HasColumnName(nameof(Bag.DispatchId                    ));
                b.Property(x => x.CountryCode                   ).HasColumnName(nameof(Bag.CountryCode                   )).HasMaxLength(2);
                b.Property(x => x.WeightPre                     ).HasColumnName(nameof(Bag.WeightPre                     )).HasPrecision(18, 3);
                b.Property(x => x.WeightPost                    ).HasColumnName(nameof(Bag.WeightPost                    )).HasPrecision(18, 3);
                b.Property(x => x.ItemCountPre                  ).HasColumnName(nameof(Bag.ItemCountPre                  ));
                b.Property(x => x.ItemCountPost                 ).HasColumnName(nameof(Bag.ItemCountPost                 ));
                b.Property(x => x.WeightVariance                ).HasColumnName(nameof(Bag.WeightVariance                )).HasPrecision(18, 3);
                b.Property(x => x.CN35No                        ).HasColumnName(nameof(Bag.CN35No                        )).HasMaxLength(20);
                b.Property(x => x.UnderAmount                   ).HasColumnName(nameof(Bag.UnderAmount                   )).HasPrecision(18, 2);
                b.HasOne<Dispatch>().WithMany().HasForeignKey(x => x.DispatchId);
                b.HasKey(x => x.Id);
            });

            builder.Entity<DispatchValidation>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "DispatchValidations");
                b.Property(x => x.CustomerCode                    ).HasColumnName(nameof(DispatchValidation.CustomerCode                    )).HasMaxLength(10);
                b.Property(x => x.DateStarted                     ).HasColumnName(nameof(DispatchValidation.DateStarted                     ));
                b.Property(x => x.DateCompleted                   ).HasColumnName(nameof(DispatchValidation.DateCompleted                   ));
                b.Property(x => x.DispatchNo                      ).HasColumnName(nameof(DispatchValidation.DispatchNo                      )).HasMaxLength(128);
                b.Property(x => x.FilePath                        ).HasColumnName(nameof(DispatchValidation.FilePath                        )).HasMaxLength(200);
                b.Property(x => x.IsFundLack                      ).HasColumnName(nameof(DispatchValidation.IsFundLack                      ));
                b.Property(x => x.IsValid                         ).HasColumnName(nameof(DispatchValidation.IsValid                         ));
                b.Property(x => x.PostalCode                      ).HasColumnName(nameof(DispatchValidation.PostalCode                      )).HasMaxLength(5);
                b.Property(x => x.ServiceCode                     ).HasColumnName(nameof(DispatchValidation.ServiceCode                     )).HasMaxLength(10);
                b.Property(x => x.ProductCode                     ).HasColumnName(nameof(DispatchValidation.ProductCode                     )).HasMaxLength(128);
                b.Property(x => x.Status                          ).HasColumnName(nameof(DispatchValidation.Status                          )).HasMaxLength(20);
                b.Property(x => x.TookInSec                       ).HasColumnName(nameof(DispatchValidation.TookInSec                       ));
                b.Property(x => x.ValidationProgress              ).HasColumnName(nameof(DispatchValidation.ValidationProgress              ));
                b.HasKey(x => x.DispatchNo);
            });

            builder.Entity<Dispatch>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Dispatches");
                b.Property(x => x.CustomerCode                    ).HasColumnName(nameof(Dispatch.CustomerCode                    )).HasMaxLength(10);
                b.Property(x => x.POBox                           ).HasColumnName(nameof(Dispatch.POBox                           )).HasMaxLength(10);
                b.Property(x => x.PPI                             ).HasColumnName(nameof(Dispatch.PPI                             )).HasMaxLength(10);
                b.Property(x => x.PostalCode                      ).HasColumnName(nameof(Dispatch.PostalCode                      )).HasMaxLength(5);
                b.Property(x => x.ServiceCode                     ).HasColumnName(nameof(Dispatch.ServiceCode                     )).HasMaxLength(10);
                b.Property(x => x.ProductCode                     ).HasColumnName(nameof(Dispatch.ProductCode                     )).HasMaxLength(128);
                b.Property(x => x.DispatchDate                    ).HasColumnName(nameof(Dispatch.DispatchDate                    ));
                b.Property(x => x.DispatchNo                      ).HasColumnName(nameof(Dispatch.DispatchNo                      )).HasMaxLength(128);
                b.Property(x => x.ETAtoHKG                        ).HasColumnName(nameof(Dispatch.ETAtoHKG                        ));
                b.Property(x => x.FlightTrucking                  ).HasColumnName(nameof(Dispatch.FlightTrucking                  )).HasMaxLength(50);
                b.Property(x => x.BatchId                         ).HasColumnName(nameof(Dispatch.BatchId                         )).HasMaxLength(35);
                b.Property(x => x.IsPayment                       ).HasColumnName(nameof(Dispatch.IsPayment                       ));
                b.Property(x => x.NoofBag                         ).HasColumnName(nameof(Dispatch.NoofBag                         ));
                b.Property(x => x.ItemCount                       ).HasColumnName(nameof(Dispatch.ItemCount                       ));
                b.Property(x => x.TotalWeight                     ).HasColumnName(nameof(Dispatch.TotalWeight                     )).HasPrecision(18, 3);
                b.Property(x => x.TotalPrice                      ).HasColumnName(nameof(Dispatch.TotalPrice                      )).HasPrecision(18, 2);
                b.Property(x => x.Status                          ).HasColumnName(nameof(Dispatch.Status                          ));
                b.Property(x => x.IsActive                        ).HasColumnName(nameof(Dispatch.IsActive                        ));
                b.Property(x => x.CN38                            ).HasColumnName(nameof(Dispatch.CN38                            )).HasMaxLength(30);
                b.Property(x => x.TransactionDateTime             ).HasColumnName(nameof(Dispatch.TransactionDateTime             ));
                b.Property(x => x.ATA                             ).HasColumnName(nameof(Dispatch.ATA                             ));
                b.Property(x => x.PostCheckTotalBags              ).HasColumnName(nameof(Dispatch.PostCheckTotalBags              ));
                b.Property(x => x.PostCheckTotalWeight            ).HasColumnName(nameof(Dispatch.PostCheckTotalWeight            )).HasPrecision(18, 3);
                b.Property(x => x.AirportHandling                 ).HasColumnName(nameof(Dispatch.AirportHandling                 ));
                b.Property(x => x.Remark                          ).HasColumnName(nameof(Dispatch.Remark                          )).HasMaxLength(100);
                b.Property(x => x.WeightGap                       ).HasColumnName(nameof(Dispatch.WeightGap                       )).HasPrecision(18, 3);
                b.Property(x => x.WeightAveraged                  ).HasColumnName(nameof(Dispatch.WeightAveraged                  )).HasPrecision(18, 3);
                b.Property(x => x.DateSOAProcessCompleted         ).HasColumnName(nameof(Dispatch.DateSOAProcessCompleted         ));
                b.Property(x => x.SOAProcessCompletedByID         ).HasColumnName(nameof(Dispatch.SOAProcessCompletedByID         ));
                b.Property(x => x.TotalWeightSOA                  ).HasColumnName(nameof(Dispatch.TotalWeightSOA                  )).HasPrecision(18, 3);
                b.Property(x => x.TotalAmountSOA                  ).HasColumnName(nameof(Dispatch.TotalAmountSOA                  )).HasPrecision(18, 2);
                b.Property(x => x.PerformanceDaysDiff             ).HasColumnName(nameof(Dispatch.PerformanceDaysDiff             ));
                b.Property(x => x.DatePerformanceDaysDiff         ).HasColumnName(nameof(Dispatch.DatePerformanceDaysDiff         ));
                b.Property(x => x.AirlineCode                     ).HasColumnName(nameof(Dispatch.AirlineCode                     )).HasMaxLength(20);
                b.Property(x => x.FlightNo                        ).HasColumnName(nameof(Dispatch.FlightNo                        )).HasMaxLength(20);
                b.Property(x => x.PortDeparture                   ).HasColumnName(nameof(Dispatch.PortDeparture                   )).HasMaxLength(50);
                b.Property(x => x.ExtDispatchNo                   ).HasColumnName(nameof(Dispatch.ExtDispatchNo                   )).HasMaxLength(50);
                b.Property(x => x.DateFlight                      ).HasColumnName(nameof(Dispatch.DateFlight                      ));
                b.Property(x => x.AirportTranshipment             ).HasColumnName(nameof(Dispatch.AirportTranshipment             )).HasMaxLength(20);
                b.Property(x => x.OfficeDestination               ).HasColumnName(nameof(Dispatch.OfficeDestination               )).HasMaxLength(30);
                b.Property(x => x.OfficeOrigin                    ).HasColumnName(nameof(Dispatch.OfficeOrigin                    )).HasMaxLength(30);
                b.Property(x => x.Stage1StatusDesc                ).HasColumnName(nameof(Dispatch.Stage1StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage2StatusDesc                ).HasColumnName(nameof(Dispatch.Stage2StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage3StatusDesc                ).HasColumnName(nameof(Dispatch.Stage3StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage4StatusDesc                ).HasColumnName(nameof(Dispatch.Stage4StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage5StatusDesc                ).HasColumnName(nameof(Dispatch.Stage5StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage6StatusDesc                ).HasColumnName(nameof(Dispatch.Stage6StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage7StatusDesc                ).HasColumnName(nameof(Dispatch.Stage7StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage8StatusDesc                ).HasColumnName(nameof(Dispatch.Stage8StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.Stage9StatusDesc                ).HasColumnName(nameof(Dispatch.Stage9StatusDesc                )).HasMaxLength(250);
                b.Property(x => x.DateStartedAPI                  ).HasColumnName(nameof(Dispatch.DateStartedAPI                  ));
                b.Property(x => x.DateEndedAPI                    ).HasColumnName(nameof(Dispatch.DateEndedAPI                    ));
                b.Property(x => x.StatusAPI                       ).HasColumnName(nameof(Dispatch.StatusAPI                       )).HasMaxLength(250);
                b.Property(x => x.CountryOfLoading                ).HasColumnName(nameof(Dispatch.CountryOfLoading                )).HasMaxLength(50);
                b.Property(x => x.DateFlightArrival               ).HasColumnName(nameof(Dispatch.DateFlightArrival               ));
                b.Property(x => x.PostManifestSuccess             ).HasColumnName(nameof(Dispatch.PostManifestSuccess             ));
                b.Property(x => x.PostManifestMsg                 ).HasColumnName(nameof(Dispatch.PostManifestMsg                 )).HasMaxLength(150);
                b.Property(x => x.PostManifestDate                ).HasColumnName(nameof(Dispatch.PostManifestDate                ));
                b.Property(x => x.PostDeclarationSuccess          ).HasColumnName(nameof(Dispatch.PostDeclarationSuccess          ));
                b.Property(x => x.PostDeclarationMsg              ).HasColumnName(nameof(Dispatch.PostDeclarationMsg              )).HasMaxLength(150);
                b.Property(x => x.PostDeclarationDate             ).HasColumnName(nameof(Dispatch.PostDeclarationDate             ));
                b.Property(x => x.AirwayBLNo                      ).HasColumnName(nameof(Dispatch.AirwayBLNo                      )).HasMaxLength(20);
                b.Property(x => x.AirwayBLDate                    ).HasColumnName(nameof(Dispatch.AirwayBLDate                    ));
                b.Property(x => x.DateLocalDelivery               ).HasColumnName(nameof(Dispatch.DateLocalDelivery               ));
                b.Property(x => x.DateCLStage1Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage1Submitted           ));
                b.Property(x => x.DateCLStage2Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage2Submitted           ));
                b.Property(x => x.DateCLStage3Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage3Submitted           ));
                b.Property(x => x.DateCLStage4Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage4Submitted           ));
                b.Property(x => x.DateCLStage5Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage5Submitted           ));
                b.Property(x => x.DateCLStage6Submitted           ).HasColumnName(nameof(Dispatch.DateCLStage6Submitted           ));
                b.Property(x => x.BRCN38RequestId                 ).HasColumnName(nameof(Dispatch.BRCN38RequestId                 )).HasMaxLength(20);
                b.Property(x => x.DateArrival                     ).HasColumnName(nameof(Dispatch.DateArrival                     ));
                b.Property(x => x.DateAcceptanceScanning          ).HasColumnName(nameof(Dispatch.DateAcceptanceScanning          ));
                b.Property(x => x.SeqNo                           ).HasColumnName(nameof(Dispatch.SeqNo                           ));
                b.Property(x => x.CORateOptionId                  ).HasColumnName(nameof(Dispatch.CORateOptionId                  )).HasMaxLength(10);
                b.Property(x => x.PaymentMode                     ).HasColumnName(nameof(Dispatch.PaymentMode                     )).HasMaxLength(10);
                b.Property(x => x.CurrencyId                      ).HasColumnName(nameof(Dispatch.CurrencyId                      )).HasMaxLength(3);
                b.Property(x => x.ImportProgress                  ).HasColumnName(nameof(Dispatch.ImportProgress                  ));
                b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerCode).HasPrincipalKey(x => x.Code);
                b.HasKey(x => x.Id);
            });

            builder.Entity<Queue>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Queues");
                b.Property(x => x.EventType).HasColumnName(nameof(Queue.EventType)).HasMaxLength(50);
                b.Property(x => x.FilePath).HasColumnName(nameof(Queue.FilePath)).HasMaxLength(512);
                b.Property(x => x.DeleteFileOnSuccess).HasColumnName(nameof(Queue.DeleteFileOnSuccess));
                b.Property(x => x.DeleteFileOnFailed).HasColumnName(nameof(Queue.DeleteFileOnFailed));
                b.Property(x => x.DateCreated).HasColumnName(nameof(Queue.DateCreated));
                b.Property(x => x.Status).HasColumnName(nameof(Queue.Status)).HasMaxLength(20);
                b.Property(x => x.TookInSec).HasColumnName(nameof(Queue.TookInSec));
                b.Property(x => x.ErrorMsg).HasColumnName(nameof(Queue.ErrorMsg)).HasMaxLength(512);
                b.Property(x => x.StartTime).HasColumnName(nameof(Queue.StartTime));
                b.Property(x => x.EndTime).HasColumnName(nameof(Queue.EndTime));
                b.HasKey(x => x.Id);
            });

            builder.Entity<Chibi>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "Chibis");
                b.Property(x => x.FileName).HasColumnName(nameof(Chibi.FileName)).HasMaxLength(128);
                b.Property(x => x.UUID).HasColumnName(nameof(Chibi.UUID)).HasMaxLength(128);
                b.Property(x => x.URL).HasColumnName(nameof(Chibi.URL)).HasMaxLength(128);
                b.Property(x => x.OriginalName).HasColumnName(nameof(Chibi.OriginalName)).HasMaxLength(128);
                b.Property(x => x.GeneratedName).HasColumnName(nameof(Chibi.GeneratedName)).HasMaxLength(128);
                b.HasKey(x => x.Id);
            });

            builder.Entity<RateWeightBreak>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "RateWeightBreaks");
                b.Property(x => x.RateId).HasColumnName(nameof(RateWeightBreak.RateId));
                b.Property(x => x.PostalOrgId).HasColumnName(nameof(RateWeightBreak.PostalOrgId));
                b.Property(x => x.WeightMin).HasColumnName(nameof(RateWeightBreak.WeightMin)).HasPrecision(18, 2).IsRequired(false);
                b.Property(x => x.WeightMax).HasColumnName(nameof(RateWeightBreak.WeightMax)).HasPrecision(18, 2).IsRequired(false);
                b.Property(x => x.ProductCode).HasColumnName(nameof(RateWeightBreak.ProductCode)).HasMaxLength(128);
                b.Property(x => x.CurrencyId).HasColumnName(nameof(RateWeightBreak.CurrencyId));
                b.Property(x => x.ItemRate).HasColumnName(nameof(RateWeightBreak.ItemRate)).HasPrecision(18, 2).IsRequired(false);
                b.Property(x => x.WeightRate).HasColumnName(nameof(RateWeightBreak.WeightRate)).HasPrecision(18, 2).IsRequired(false);
                b.Property(x => x.IsExceedRule).HasColumnName(nameof(RateWeightBreak.IsExceedRule));
                b.Property(x => x.PaymentMode).HasColumnName(nameof(RateWeightBreak.PaymentMode)).HasMaxLength(128);
                b.Property(x => x.Zone).HasColumnName(nameof(RateWeightBreak.Zone)).HasMaxLength(58);
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
                b.Property(x => x.ProductCode).HasColumnName(nameof(Postal.ProductCode)).HasMaxLength(128);
                b.Property(x => x.ProductDesc).HasColumnName(nameof(Postal.ProductDesc)).HasMaxLength(30);
                b.Property(x => x.ItemTopUpValue).HasColumnName(nameof(Postal.ItemTopUpValue)).HasPrecision(18, 2);
                b.HasKey(x => x.Id);
            });

            builder.Entity<RateItem>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "RateItems");
                b.Property(x => x.RateId).HasColumnName(nameof(RateItem.RateId));
                b.Property(x => x.ServiceCode).HasColumnName(nameof(RateItem.ServiceCode)).HasMaxLength(2);
                b.Property(x => x.ProductCode).HasColumnName(nameof(RateItem.ProductCode)).HasMaxLength(128);
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
                b.Property(x => x.Service).HasColumnName(nameof(Rate.Service)).HasMaxLength(2);
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
                b.Property(x => x.ClientSecret).HasColumnName(nameof(Customer.ClientSecret)).HasMaxLength(100);
                b.Property(x => x.ClientKey).HasColumnName(nameof(Customer.ClientKey)).HasMaxLength(100);
                b.Property(x => x.APIAccessToken).HasColumnName(nameof(Customer.APIAccessToken)).HasMaxLength(250);
                b.HasKey(x => x.Id);
                b.HasAlternateKey(x => x.Code);
            });

            builder.Entity<CustomerTransaction>(b =>
            {
                b.ToTable(SMPConsts.DbTablePrefix + "CustomerTransactions");
                b.Property(x => x.Wallet).HasColumnName(nameof(CustomerTransaction.Wallet)).HasMaxLength(128);
                b.Property(x => x.Customer).HasColumnName(nameof(CustomerTransaction.Customer)).HasMaxLength(10);
                b.Property(x => x.PaymentMode).HasColumnName(nameof(CustomerTransaction.PaymentMode)).HasMaxLength(20);
                b.Property(x => x.Currency).HasColumnName(nameof(CustomerTransaction.Currency)).HasMaxLength(3);
                b.Property(x => x.TransactionType).HasColumnName(nameof(CustomerTransaction.TransactionType)).HasMaxLength(100);
                b.Property(x => x.Amount).HasColumnName(nameof(CustomerTransaction.Amount)).HasPrecision(18, 2);
                b.Property(x => x.ReferenceNo).HasColumnName(nameof(CustomerTransaction.ReferenceNo)).HasMaxLength(100);
                b.Property(x => x.Description).HasColumnName(nameof(CustomerTransaction.Description)).HasMaxLength(521);
                b.Property(x => x.TransactionDate).HasColumnName(nameof(CustomerTransaction.TransactionDate));
            });
        }
    }
}
