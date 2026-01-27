using DailyJournalApp.Services;

namespace DailyJournalApp;

public partial class AppShell : Shell
{
    private readonly SecurityService _securityService;

	public AppShell(SecurityService securityService)
	{
        _securityService = securityService;
		InitializeComponent();

        Routing.RegisterRoute("primary_login", typeof(DailyJournalApp.Views.LoginPage));
        Routing.RegisterRoute("setup_entry", typeof(DailyJournalApp.Views.SetupPage));
	}

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (confirm)
        {
            _securityService.Logout();
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync("//HomePage");
        }
    }
}
