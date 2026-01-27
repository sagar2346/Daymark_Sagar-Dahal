using DailyJournalApp.ViewModels;
using CommunityToolkit.Maui.Behaviors; // Needed for EventToCommandBehavior if using toolkit, otherwise we need to add package or specific code

namespace DailyJournalApp.Views;

public partial class JournalPage : ContentPage
{
    private readonly JournalViewModel _viewModel;

	public JournalPage(JournalViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
