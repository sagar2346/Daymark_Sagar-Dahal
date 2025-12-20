using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Services;
using DailyJournalApp.Models;

namespace DailyJournalApp.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ExportService _exportService;
        private readonly JournalService _journalService;

        [ObservableProperty]
        private bool isDarkMode;

        public SettingsViewModel(ExportService exportService, JournalService journalService)
        {
            _exportService = exportService;
            _journalService = journalService;
            Title = "Settings";

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
                var entries = await _journalService.GetAllEntriesAsync();
                
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
    }
}
