using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ExportService _exportService;
        private readonly JournalService _journalService;
        private readonly SecurityService _securityService;

        [ObservableProperty]
        private bool isDarkMode;

        [ObservableProperty]
        private string currentUserName;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private string confirmNewPassword;

        public SettingsViewModel(ExportService exportService, JournalService journalService, SecurityService securityService)
        {
            _exportService = exportService;
            _journalService = journalService;
            _securityService = securityService;
            Title = "Settings";

            // Load saved theme preference
            IsDarkMode = Preferences.Default.Get("IsDarkMode", Application.Current.RequestedTheme == AppTheme.Dark);
            
            CurrentUserName = Preferences.Default.Get("CurrentUser", "User");
            UserIsAuthenticated = _securityService.IsAuthenticated;

            CurrentUserName = Preferences.Default.Get("CurrentUser", "User");
        }partial void OnIsDarkModeChanged(bool value)
        {
            Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            Preferences.Default.Set("IsDarkMode", value);
        }

        [RelayCommand]
        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }



        [RelayCommand]
        public async Task ExportDataAsync()
        {
            IsBusy = true;
            try
            {
                var entries = await _journalService.GetAllEntriesAsync();
                
                // On Windows/Desktop, we can save to a specific path
                string fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd}.pdf";
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fullPath = Path.Combine(folder, fileName);

                await _exportService.ExportJournalToPdfAsync(entries, fullPath);

                await AppShell.Current.DisplayAlert("Export Successful", $"Your journal has been saved to: {fullPath}", "OK");
            }
            catch (Exception ex)
            {
                await AppShell.Current.DisplayAlert("Export Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task UpdatePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                await AppShell.Current.DisplayAlert("Error", "Password must be at least 6 characters.", "OK");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                await AppShell.Current.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                await _securityService.UpdatePasswordAsync(NewPassword);
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                await AppShell.Current.DisplayAlert("Success", "Your login password has been updated.", "OK");
            }
            catch (Exception ex)
            {
                await AppShell.Current.DisplayAlert("Error", $"Failed to update password: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            bool confirm = await AppShell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                _securityService.Logout();
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await Shell.Current.GoToAsync("//HomePage");
                });
            }
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            await Shell.Current.GoToAsync("primary_login");
        }
    }
}
