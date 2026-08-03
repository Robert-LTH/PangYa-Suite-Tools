using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PangyaAPI.SQL;
using PangyaAPI.SQL.EntityFramework;

namespace PangyaAPI.Network.Hosting
{
    public static class EntityFrameworkServiceCollectionExtensions
    {
        public static IServiceCollection AddPangyaEntityFramework(
            this IServiceCollection services, DatabaseOptions database)
        {
            services.AddPooledDbContextFactory<PangyaDbContext>(options =>
                PangyaDbContextOptions.Configure(options, database));
            return services;
        }
    }
}
