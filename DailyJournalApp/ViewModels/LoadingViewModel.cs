using CommunityToolkit.Mvvm.ComponentModel;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class LoadingViewModel : BaseViewModel
    {
        private readonly SecurityService _securityService;

        public LoadingViewModel(SecurityService securityService)
        {
            _securityService = securityService;
        }

        public async Task CheckAuthAsync()
        {
            // Small delay to ensure UI is ready and database is init
            await Task.Delay(500);

            await Shell.Current.GoToAsync("//HomePage");
        }
    }
}
