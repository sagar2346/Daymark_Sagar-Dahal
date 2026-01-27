using DailyJournalApp.Services;

namespace DailyJournalApp;

public partial class App : Application
{
    private readonly SecurityService _securityService;

	public App(SecurityService securityService)
	{
		InitializeComponent();
        _securityService = securityService;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell(_securityService));
	}
}
