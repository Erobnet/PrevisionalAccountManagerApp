using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using PrevisionalAccountManager.Services;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager
{
    public partial class App : Application
    {
        public static readonly string AppSpecificPath =
#if DEBUG
            Environment.CurrentDirectory;
#else
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrevisionalAccountManager");
#endif
        private IHost? _host;
        private static IServiceProvider? _serviceProvider;

        public static T GetRequiredInstance<T>()
        {
            return _serviceProvider.GetRequiredInstance<T>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Directory.CreateDirectory(AppSpecificPath);
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
            _serviceProvider = _host.Services;
            var databaseCtx = GetRequiredInstance<DatabaseContext>();
            databaseCtx.CheckMigration();
            var culture = new CultureInfo("fr-FR");
            ApplyGlobalCultureSettings(culture);
            base.OnStartup(e);
        }

        private static void ApplyGlobalCultureSettings(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Apply globally
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // Register your services
            services.AddSingleton(new DatabaseContext { });
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IStyleService, StyleService>();
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<ITransactionService, TransactionService>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }

    public static class DependencyResolverHelpers
    {
        public static T GetRequiredInstance<T>(this IServiceProvider? serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            var service = serviceProvider.GetService<T>();
            ValidateServiceNotNull(service);
            return service;
        }

        private static void ValidateServiceNotNull<T>([NotNull] T? service)
        {
            if ( service == null )
                throw new NotSupportedException($"Dependency injection {typeof(T).FullName} was not configured.");
        }
    }
}