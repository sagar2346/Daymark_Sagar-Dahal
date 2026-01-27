using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Maui.LifecycleEvents;

namespace DailyJournalApp;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .UseLiveCharts()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if WINDOWS
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
#endif
        });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<DailyJournalApp.Services.DatabaseService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.JournalService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.ExportService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.AuthService>();

        
        // ViewModels
        builder.Services.AddTransient<DailyJournalApp.ViewModels.JournalViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.DashboardViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.TimelineViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.AnalyticsViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.SettingsViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.LoginViewModel>();


        // Views
        builder.Services.AddTransient<DailyJournalApp.Views.JournalPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.DashboardPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.TimelinePage>();
        builder.Services.AddTransient<DailyJournalApp.Views.AnalyticsPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.SettingsPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.LoginPage>();


        var app = builder.Build();
        Services = app.Services;

        // Set QuestPDF License globally after build
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        return app;
    }
}
