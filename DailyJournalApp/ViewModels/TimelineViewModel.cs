using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    public partial class TimelineViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        private List<JournalEntry> _allEntries = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        public ObservableCollection<JournalEntry> Entries { get; } = new();

        private readonly SecurityService _securityService;

        public TimelineViewModel(JournalService journalService, SecurityService securityService)
        {
            _journalService = journalService;
            _securityService = securityService;
            Title = "Timeline";
            UserIsAuthenticated = _securityService.IsAuthenticated;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterEntries();
        }

        private void FilterEntries()
        {
            try
            {
                var search = SearchText ?? string.Empty;
                var filtered = string.IsNullOrWhiteSpace(search) 
                    ? _allEntries 
                    : _allEntries.Where(e => (e.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) || 
                                             (e.Content?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

                Entries.Clear();
                foreach (var entry in filtered)
                {
                    Entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtering error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task LoadEntriesAsync()
        {
            try
            {
                IsBusy = true;
                _allEntries = await _journalService.GetAllEntriesAsync();
                FilterEntries();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load timeline: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            if (entry == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this entry?", "Yes", "No");
            if (confirm)
            {
                await _journalService.DeleteEntryAsync(entry);
                await LoadEntriesAsync();
            }
        }
    }
}
