using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Login Page. Handles password setup for new users
    /// and secure verification for existing users.
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string userName = "Sagar";

        [ObservableProperty]
        private string errorMessage = string.Empty;

        /// <summary>
        /// True if the app is being used for the first time (no password stored).
        /// </summary>
        [ObservableProperty]
        private bool isSetupMode;

        /// <summary>
        /// Logic helper for UI triggers to display error messages.
        /// </summary>
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            Title = "Authentication";
            
            // Automatically determine if we should show 'Setup' or 'Login' screen
            IsSetupMode = !_authService.IsPasswordSet();
        }

        /// <summary>
        /// Executes the login or password setup logic.
        /// Redirects to the Dashboard upon successful authentication.
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(HasError));

            // Basic Validation
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
                    // Case 1: First-time user setup
                    _authService.SetPassword(Password);
                    _authService.SetCurrentUserName(UserName);
                    await AppShell.Current.DisplayAlert("Setup Complete", "Your security password has been saved.", "Get Started");
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
                else
                {
                    // Case 2: Standard authentication check
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
                // Ensure state is reset even if navigation fails
                IsBusy = false;
            }
        }
    }
}
