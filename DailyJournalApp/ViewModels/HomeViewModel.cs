using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly SecurityService _securityService;

        public HomeViewModel(SecurityService securityService)
        {
            _securityService = securityService;
            Title = "Welcome";
        }

        [RelayCommand]
        private async Task GetStarted()
        {
            try 
            {
                Console.WriteLine("DEBUG: GetStarted clicked");
                bool isSignedUp = await _securityService.IsSignedUpAsync();
                Console.WriteLine($"DEBUG: IsSignedUp: {isSignedUp}");
                
                if (isSignedUp)
                {
                    Console.WriteLine("DEBUG: Navigating to primary_login");
                    await Shell.Current.GoToAsync("primary_login");
                }
                else
                {
                    Console.WriteLine("DEBUG: Navigating to setup_entry");
                    await Shell.Current.GoToAsync("setup_entry");
                }
                Console.WriteLine("DEBUG: Navigation initiated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Navigation Crashed: {ex}");
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Navigation Error", ex.Message, "OK");
                }
            }
        }
    }
}
