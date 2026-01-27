using DailyJournalApp.Services;

namespace DailyJournalApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        #if WINDOWS
        window.Title = "Daily Journal";
        #endif
        return window;
    }
}
