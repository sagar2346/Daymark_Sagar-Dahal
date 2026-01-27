using DailyJournalApp.Services;

namespace DailyJournalApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("dashboard", typeof(Views.DashboardPage));
        
        // Ensure the app starts on the Login page
        CurrentItem = loginItem;
    }

    private void CheckAuthentication()
    {
        var authService = Handler?.MauiContext?.Services.GetService<AuthService>() 
                         ?? MauiProgram.Services.GetService<AuthService>(); // Fallback if handler not yet ready
        
        if (authService != null)
        {
            authService.Logout(); // Force login every time the app starts as requested
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (!answer) return;

        var authService = Handler?.MauiContext?.Services.GetService<AuthService>() 
                         ?? MauiProgram.Services.GetService<AuthService>();
        authService?.Logout();

        // Close flyout first to prevent navigation issues on some platforms
        FlyoutIsPresented = false;
        
        // Use Dispatcher to ensure stable navigation back to login
        Dispatcher.Dispatch(() => {
            CurrentItem = loginItem;
        });
    }
}
