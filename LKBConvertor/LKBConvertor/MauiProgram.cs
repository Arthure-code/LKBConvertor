using LKBConvertor.Data;
using LKBConvertor.Models;
using LKBConvertor.Services;
using LKBConvertor.ViewModels;
using LKBConvertor.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using UraniumUI;
using UraniumUI.Icons.FontAwesome;

namespace LKBConvertor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider
                .RegisterLicense(LireCle());

            var builder = MauiApp.CreateBuilder();
            builder.ConfigureSyncfusionCore();
            builder.ConfigureSyncfusionToolkit();

            builder
                .UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFontAwesomeIconFonts();
                });

            var services = builder.Services;

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<LKBDatabase>();
            // ConversionService : plus enregistré, méthodes toutes statiques.

            services.AddTransient<HomeViewModel>();
            services.AddTransient<HistoriqueViewModel>();

            services.AddTransient<HomePage>();
            services.AddTransient<HistoriquePage>();

            services.AddTransient<Func<ConversionType, ConversionPage>>(sp =>
                type => ActivatorUtilities.CreateInstance<ConversionPage>(sp, type));
            services.AddTransient<Func<string, PdfViewerPage>>(sp =>
                path => ActivatorUtilities.CreateInstance<PdfViewerPage>(sp, path));

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        private static string LireCle()
        {
            using var stream = FileSystem
                .OpenAppPackageFileAsync("appsettings.json")
                .GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var config = System.Text.Json.JsonSerializer
                .Deserialize<AppConfig>(json);
            return config?.SyncfusionKey ?? string.Empty;
        }
    }

    public record AppConfig(string? SyncfusionKey);
}
