using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PangyaAPI.IFF;
using PangyaAPI.Network.Configuration;

namespace PangyaAPI.Network.Hosting
{
    public interface IIffCatalogProvider
    {
        bool IsLoaded { get; }
        IffCatalog Catalog { get; }
    }

    internal sealed class IffCatalogProvider : IIffCatalogProvider
    {
        private IffCatalog _catalog;
        public bool IsLoaded => _catalog is not null;
        public IffCatalog Catalog => _catalog ?? throw new InvalidOperationException("The IFF catalog has not been loaded.");

        public async Task InitializeAsync(IffOptions options, CancellationToken cancellationToken)
        {
            if (_catalog is not null) return;
            _catalog = await IffCatalog.LoadAsync(options.Path,
                new IffCatalogOptions(options.Region, options.RequiredTables), cancellationToken).ConfigureAwait(false);
        }
    }

    internal sealed class IffCatalogHostedService(IffCatalogProvider provider, IffOptions options) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => provider.InitializeAsync(options, cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public static class IffCatalogServiceCollectionExtensions
    {
        public static IServiceCollection AddPangyaIffCatalog(this IServiceCollection services, IffOptions options)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(options);
            services.AddSingleton(options);
            services.AddSingleton<IffCatalogProvider>();
            services.AddSingleton<IIffCatalogProvider>(provider => provider.GetRequiredService<IffCatalogProvider>());
            services.AddHostedService<IffCatalogHostedService>();
            services.AddSingleton<CharacterEquipmentService>();
            return services;
        }
    }
}
