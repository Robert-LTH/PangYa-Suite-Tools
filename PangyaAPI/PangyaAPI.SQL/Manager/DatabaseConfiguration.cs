using System;
using PangyaAPI.SQL.EntityFramework;
namespace PangyaAPI.SQL
{
    public static class DatabaseConfiguration
    {
        private static DatabaseOptions current;
        private static IRelationalCommandExecutor executor;

        public static void Configure(DatabaseOptions options)
        {
            current = options ?? throw new ArgumentNullException(nameof(options));
            DatabaseConnectionStringFactory.Validate(options);
            executor = null;
        }

        public static void ConfigureExecutor(IRelationalCommandExecutor commandExecutor)
        {
            executor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        }

        public static DatabaseOptions Current => current
            ?? throw new InvalidOperationException("Database configuration has not been initialized.");

        public static IRelationalCommandExecutor Executor
        {
            get
            {
                if (executor != null)
                    return executor;

                var options = Current;
                executor = new EfRelationalCommandExecutor(new PangyaDbContextFactory(options), options);
                return executor;
            }
        }
    }
}
