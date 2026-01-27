using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views;

public partial class TimelinePage : ContentPage
{
    private readonly TimelineViewModel _viewModel;

	public TimelinePage(TimelineViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadEntriesAsync();
    }
}
