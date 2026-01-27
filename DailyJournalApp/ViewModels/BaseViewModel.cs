using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyJournalApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private bool userIsAuthenticated;

        public bool IsNotBusy => !IsBusy;
    }
}
