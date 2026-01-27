using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class SetupPage : ContentPage
{
	public SetupPage(SetupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
