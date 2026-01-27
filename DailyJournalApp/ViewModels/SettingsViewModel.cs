using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ExportService _exportService;
        private readonly JournalService _journalService;

        [ObservableProperty]
        private bool isDarkMode;

        [ObservableProperty]
        private DateTime exportStartDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime exportEndDate = DateTime.Today;

        [ObservableProperty]
        private string newPassword = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string userName = "Sagar";

        [ObservableProperty]
        private string userProfileType = "Personal Profile";

        public SettingsViewModel(ExportService exportService, JournalService journalService)
        {
            _exportService = exportService;
            _journalService = journalService;
            Title = "Settings";
            Initialize();
        }

        [RelayCommand]
        public void Initialize()
        {
            // Load saved theme preference
            IsDarkMode = Preferences.Default.Get("IsDarkMode", Application.Current.RequestedTheme == AppTheme.Dark);
        }

        partial void OnIsDarkModeChanged(bool value)
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
                var allEntries = await _journalService.GetAllEntriesAsync();
                var entries = allEntries.Where(e => e.EntryDate.Date >= ExportStartDate.Date && 
                                                   e.EntryDate.Date <= ExportEndDate.Date).ToList();

                if (!entries.Any())
                {
                    await AppShell.Current.DisplayAlert("Info", "No entries found in the selected date range.", "OK");
                    return;
                }
                
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
        public async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                await AppShell.Current.DisplayAlert("Error", "Please enter a new password.", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await AppShell.Current.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
            if (authService != null)
            {
                authService.SetPassword(NewPassword);
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                await AppShell.Current.DisplayAlert("Success", "Password changed successfully!", "OK");
            }
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            bool answer = await AppShell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (!answer) return;

            var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
            authService?.Logout();

            if (Shell.Current is AppShell shell)
            {
                shell.FlyoutIsPresented = false;
                shell.Dispatcher.Dispatch(() => {
                    shell.CurrentItem = shell.FindByName<ShellItem>("loginItem");
                });
            }
        }
    }
}
