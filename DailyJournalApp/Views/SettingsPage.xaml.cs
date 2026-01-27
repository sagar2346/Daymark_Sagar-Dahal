using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as SettingsViewModel)?.InitializeCommand.Execute(null);
    }
}
