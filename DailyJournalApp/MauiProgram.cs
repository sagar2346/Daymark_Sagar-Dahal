using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;

namespace DailyJournalApp;

public static class MauiProgram
{
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

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<DailyJournalApp.Services.DatabaseService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.SecurityService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.JournalService>();
        builder.Services.AddSingleton<DailyJournalApp.Services.ExportService>();

        
        // ViewModels
        builder.Services.AddTransient<DailyJournalApp.ViewModels.LoadingViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.HomeViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.LoginViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.SetupViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.JournalViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.DashboardViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.TimelineViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.AnalyticsViewModel>();
        builder.Services.AddTransient<DailyJournalApp.ViewModels.SettingsViewModel>();

        // Views
        builder.Services.AddTransient<DailyJournalApp.Views.LoadingPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.HomePage>();
        builder.Services.AddTransient<DailyJournalApp.Views.LoginPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.LoginPopupPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.SetupPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.JournalPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.DashboardPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.TimelinePage>();
        builder.Services.AddTransient<DailyJournalApp.Views.AnalyticsPage>();
        builder.Services.AddTransient<DailyJournalApp.Views.SettingsPage>();

        var app = builder.Build();

        // Set QuestPDF License globally after build
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        return app;
    }
}
