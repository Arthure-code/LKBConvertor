using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

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

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        private static string LireCle()
        {
            using var stream = FileSystem
                .OpenAppPackageFileAsync("appsettings.json").Result;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var config = System.Text.Json.JsonSerializer
                .Deserialize<AppConfig>(json);
            return config?.SyncfusionKey ?? string.Empty;
        }
    }
}

public record AppConfig(string SyncfusionKey);