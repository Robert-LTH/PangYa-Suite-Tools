using System;
using Microsoft.EntityFrameworkCore;

namespace PangyaAPI.SQL.EntityFramework
{
    public sealed class PangyaDbContextFactory
    {
        private readonly DatabaseOptions _database;

        public PangyaDbContextFactory(DatabaseOptions database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            DatabaseConnectionStringFactory.Validate(database);
        }

        public PangyaDbContext CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<PangyaDbContext>();
            PangyaDbContextOptions.Configure(builder, _database);
            return new PangyaDbContext(builder.Options);
        }
    }
}
