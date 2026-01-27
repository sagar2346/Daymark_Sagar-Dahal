using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;
using Markdig;
using System.Collections.ObjectModel;

namespace DailyJournalApp.ViewModels
{
    public partial class JournalViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private DateTime selectedDate;

        [ObservableProperty]
        private JournalEntry currentEntry;

        [ObservableProperty]
        private string markdownText;

        [ObservableProperty]
        private string htmlPreview;

        [ObservableProperty]
        private Mood selectedMood;
        
        public ObservableCollection<Mood> Moods { get; } = new();

        private readonly SecurityService _securityService;

        public JournalViewModel(JournalService journalService, SecurityService securityService)
        {
            _journalService = journalService;
            _securityService = securityService;
            Title = "Journal";
            SelectedDate = DateTime.Today;
            UserIsAuthenticated = _securityService.IsAuthenticated;
            // Initialize with an empty entry to avoid nulls before load
            CurrentEntry = new JournalEntry { EntryDate = DateTime.Today };
        }

        // Called via EventToCommand or similar when page appears
        [RelayCommand]
        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                // Always reload Moods to reflect potential database updates
                Moods.Clear();
                var moods = await _journalService.GetMoodsAsync();
                foreach (var m in moods) Moods.Add(m);

                await LoadEntryArgs(SelectedDate);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DateChanged()
        {
             await LoadEntryArgs(SelectedDate);
        }

        private async Task LoadEntryArgs(DateTime date)
        {
            var entry = await _journalService.GetEntryByDateAsync(date.Date);
            if (entry != null)
            {
                CurrentEntry = entry;
                MarkdownText = entry.Content;
                SelectedMood = Moods.FirstOrDefault(m => m.Name == entry.PrimaryMood);
            }
            else
            {
                // New blank entry
                CurrentEntry = new JournalEntry { EntryDate = date.Date };
                MarkdownText = string.Empty;
                SelectedMood = null;
            }
            UpdatePreview();
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (CurrentEntry == null) return;

                CurrentEntry.Content = MarkdownText;
                CurrentEntry.PrimaryMood = SelectedMood?.Name;
                CurrentEntry.Title = string.IsNullOrWhiteSpace(CurrentEntry.Title) ? "Untitled" : CurrentEntry.Title;

                await _journalService.SaveEntryAsync(CurrentEntry);
                await Shell.Current.DisplayAlert("Success", "Entry saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteAsync()
        {
            if (CurrentEntry == null || CurrentEntry.Id == 0) return;

            var confirm = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this entry?", "Yes", "No");
            if (confirm)
            {
                await _journalService.DeleteEntryAsync(CurrentEntry);
                await LoadEntryArgs(SelectedDate); // Reload to clear
            }
        }

        // Helper to update preview when MarkdownText changes
        partial void OnMarkdownTextChanged(string value)
        {
            UpdatePreview();
        }

        [RelayCommand]
        public void ClearMood()
        {
            SelectedMood = null;
        }

        private void UpdatePreview()
        {
             if (string.IsNullOrWhiteSpace(MarkdownText))
             {
                 HtmlPreview = string.Empty;
                 return;
             }
             var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
             HtmlPreview = Markdown.ToHtml(MarkdownText, pipeline);
        }
    }
}
