using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel _viewModel;

	public LoadingPage(LoadingViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckAuthAsync();
    }
}
