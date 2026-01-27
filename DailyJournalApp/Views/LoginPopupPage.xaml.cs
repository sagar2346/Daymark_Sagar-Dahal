using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class LoginPopupPage : ContentPage
{
	public LoginPopupPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
