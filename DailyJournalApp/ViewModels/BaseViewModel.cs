using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyJournalApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty]
        private string title = string.Empty;



        public bool IsNotBusy => !IsBusy;
    }
}
