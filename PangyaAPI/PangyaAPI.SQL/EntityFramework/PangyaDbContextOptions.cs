using System;
using Microsoft.EntityFrameworkCore;
using PangyaAPI.SQL.Resources;

namespace PangyaAPI.SQL.EntityFramework
{
    public static class PangyaDbContextOptions
    {
        public static void Configure(DbContextOptionsBuilder optionsBuilder, DatabaseOptions database)
        {
            if (optionsBuilder == null)
                throw new ArgumentNullException(nameof(optionsBuilder));
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var connectionString = DatabaseConnectionStringFactory.Create(database);

            switch (database.Engine.ToUpperInvariant())
            {
                case "MSSQL":
                case "SQLSERVER":
                    optionsBuilder.UseSqlServer(connectionString, provider =>
                        provider.MigrationsAssembly("PangyaAPI.Migrations.SqlServer").EnableRetryOnFailure());
                    break;
                case "MYSQL":
                    optionsBuilder.UseMySQL(connectionString, provider =>
                        provider.MigrationsAssembly("PangyaAPI.Migrations.MySql"));
                    break;
                case "POSTGRESQL":
                case "PGSQL":
                    optionsBuilder.UseNpgsql(connectionString, provider =>
                        provider.MigrationsAssembly("PangyaAPI.Migrations.PostgreSql").EnableRetryOnFailure());
                    break;
                default:
                    throw new NotSupportedException(SqlMessages.Format("EngineUnsupportedEf", database.Engine));
            }

            optionsBuilder.EnableDetailedErrors();
            if (database.LogCommands)
                optionsBuilder.EnableSensitiveDataLogging(false);
        }
    }
}
