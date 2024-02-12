using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace SND.SMP.EntityFrameworkCore
{
    public static class SMPDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<SMPDbContext> builder, string connectionString)
        {
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            builder.UseMySql(connectionString, serverVersion);
        }

        public static void Configure(DbContextOptionsBuilder<SMPDbContext> builder, DbConnection connection)
        {
            var serverVersion = ServerVersion.AutoDetect(connection.ConnectionString);
            builder.UseMySql(connection, serverVersion);
        }
    }
}
