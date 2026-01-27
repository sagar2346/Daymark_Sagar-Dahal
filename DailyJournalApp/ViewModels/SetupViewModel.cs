using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class SetupViewModel : BaseViewModel
    {
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private string errorMessage;

        public SetupViewModel(SecurityService securityService)
        {
            _securityService = securityService;
            Title = "Create Profile";
        }

        [RelayCommand]
        public async Task SignupAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            try
            {
                IsBusy = true;
                await _securityService.SignupAsync(Username, Password);
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Signup failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
