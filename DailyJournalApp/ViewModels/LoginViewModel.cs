using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string userName = "Sagar";

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isSetupMode;

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            Title = "Authentication";
            IsSetupMode = !_authService.IsPasswordSet();
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(HasError));

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter a password.";
                OnPropertyChanged(nameof(HasError));
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (IsSetupMode)
                {
                    _authService.SetPassword(Password);
                    _authService.SetCurrentUserName(UserName);
                    await AppShell.Current.DisplayAlert("Success", "Password set successfully!", "OK");
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
                else
                {
                    if (_authService.VerifyPassword(Password))
                    {
                        _authService.SetCurrentUserName(UserName);
                        await Shell.Current.GoToAsync("//DashboardPage");
                    }
                    else
                    {
                        ErrorMessage = "Incorrect password. Please try again.";
                        OnPropertyChanged(nameof(HasError));
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
