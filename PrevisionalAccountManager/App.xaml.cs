using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using PrevisionalAccountManager.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.ViewModels;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;
using ProgressBar = ModernWpf.Controls.ProgressBar;

namespace PrevisionalAccountManager
{
    public partial class App : Application
    {
        [STAThread]
        private static void Main(string[] args)
        {
            VelopackApp.Build().Run();
            App app = new();
            app.InitializeComponent();
            app.Run();
        }

        public static async Task CheckForApplicationUpdate()
        {
            var updateSource = new GithubSource("https://github.com/Erobnet/PrevisionalAccountManagerApp/", null, false);
            await UpdateMyApp(updateSource);
        }

        private static async Task UpdateMyApp(IUpdateSource updateSource)
        {
            IVelopackLocator velopackLocator = VelopackLocator.Current;
            var mgr = new UpdateManager(updateSource, locator: velopackLocator);

            // check for new version
            try
            {
                var newVersion = await mgr.CheckForUpdatesAsync();
                if ( newVersion == null )
                    return; // no update available

                if ( MessageBox.Show(Current.MainWindow, "A new version is available, the installing process will begin", "Update available", MessageBoxButton.OK) == MessageBoxResult.OK )
                {
                    var mainVm = (Current.MainWindow.DataContext as MainWindowViewModel);
                    var mainVmCurrentViewModel = new ProgressBarViewModel();
                    mainVm.CurrentViewTemplate = Current.MainWindow?.FindResource("ProgressBarTemplate") as DataTemplate;
                    mainVm.CurrentViewModel = mainVmCurrentViewModel;

                    // download new version
                    await mgr.DownloadUpdatesAsync(newVersion, (step => mainVmCurrentViewModel.ProgressValue = step));
                    // install new version and restart app
                    mgr.ApplyUpdatesAndRestart(newVersion);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

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