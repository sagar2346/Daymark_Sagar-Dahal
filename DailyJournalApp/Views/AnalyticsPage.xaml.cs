using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _viewModel;

	public AnalyticsPage(AnalyticsViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAnalyticsAsync();
    }
}
