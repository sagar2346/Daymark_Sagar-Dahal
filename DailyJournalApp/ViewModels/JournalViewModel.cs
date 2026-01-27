using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;
using Markdig;
using System.Collections.ObjectModel;

namespace DailyJournalApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Journal Entry/Editing Page. 
    /// Manages rich text (Markdown), mood selection, tagging, and persistence logic.
    /// Implements IQueryAttributable for deep-linking (navigating to specific dates).
    /// </summary>
    public partial class JournalViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly JournalService _journalService;
        
        // Prevents UI race conditions during rapid data loading
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        // --- Observable Properties (UI Bound) ---

        [ObservableProperty]
        private DateTime selectedDate;

        [ObservableProperty]
        private JournalEntry currentEntry = new();

        [ObservableProperty]
        private string markdownText = string.Empty;

        [ObservableProperty]
        private string htmlPreview = string.Empty;

        [ObservableProperty]
        private Mood? selectedMood;

        [ObservableProperty]
        private string newTagName = string.Empty;

        [ObservableProperty]
        private string newSecondaryFeeling = string.Empty;

        // --- Collections ---
        
        public ObservableCollection<Mood> Moods { get; } = new();
        public ObservableCollection<string> SelectedSecondaryMoods { get; } = new();
        public ObservableCollection<Tag> SelectedTags { get; } = new();
        public ObservableCollection<Mood> MoodSuggestions { get; } = new();

        public JournalViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Journal";
            
            // Set default view to today
            SelectedDate = DateTime.Today;
            CurrentEntry = new JournalEntry { EntryDate = DateTime.Today };
        }

        /// <summary>
        /// Initial load method. Fetches all master-data (Moods) 
        /// and then loads the entry for the current specific date.
        /// </summary>
        [RelayCommand]
        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Moods.Clear();
                MoodSuggestions.Clear();
                
                var moods = await _journalService.GetMoodsAsync();
                foreach (var m in moods) 
                {
                    Moods.Add(m);
                    MoodSuggestions.Add(m);
                }

                await LoadEntryArgs(SelectedDate);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Triggered when the user picks a different date from the Calendar/DatePicker.
        /// </summary>
        [RelayCommand]
        public async Task DateChanged()
        {
             await LoadEntryArgs(SelectedDate);
        }

        /// <summary>
        /// Internal method to hydrate the UI with an entry's data (Content, Tags, Moods).
        /// </summary>
        private async Task LoadEntryArgs(DateTime date)
        {
            await _loadLock.WaitAsync();
            try
            {
                var entry = await _journalService.GetEntryByDateAsync(date.Date);
                SelectedTags.Clear();
                SelectedSecondaryMoods.Clear();

                if (entry != null)
                {
                    CurrentEntry = entry;
                    MarkdownText = entry.Content;
                    SelectedMood = Moods.FirstOrDefault(m => m.Name == entry.PrimaryMood);
                    
                    // Load associated Tags
                    var tags = await _journalService.GetTagsForEntryAsync(entry.Id);
                    foreach (var t in tags) SelectedTags.Add(t);

                    // Hydrate secondary moods from comma-separated string
                    if (!string.IsNullOrEmpty(entry.SecondaryMoods))
                    {
                        var names = entry.SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(n => n.Trim())
                                                     .Distinct()
                                                     .ToList();
                        foreach (var name in names)
                        {
                             SelectedSecondaryMoods.Add(name);
                        }
                    }
                }
                else
                {
                    // Case: No entry exists for this date. Initialize a blank slate.
                    CurrentEntry = new JournalEntry { EntryDate = date.Date };
                    MarkdownText = string.Empty;
                    SelectedMood = null;
                }
                UpdatePreview();
            }
            finally
            {
                _loadLock.Release();
            }
        }

        /// <summary>
        /// Validates and saves the current state to the local database.
        /// </summary>
        [RelayCommand]
        public async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (CurrentEntry == null) return;

                // Quality Check: Ensure "one entry per day" constraint is respected
                if (CurrentEntry.Id == 0)
                {
                    var existing = await _journalService.GetEntryByDateAsync(CurrentEntry.EntryDate);
                    if (existing != null)
                    {
                        MainThread.BeginInvokeOnMainThread(async () => {
                            await Shell.Current.DisplayAlert("Journal Limit", "Only one entry allowed per day.", "OK");
                        });
                        return;
                    }
                }

                // Default title if left blank
                if (string.IsNullOrWhiteSpace(CurrentEntry.Title))
                {
                    CurrentEntry.Title = "Day Log: " + SelectedDate.ToShortDateString();
                }

                // Transfer local VM properties back to the model before saving
                CurrentEntry.Content = MarkdownText;
                CurrentEntry.PrimaryMood = SelectedMood?.Name;
                
                // Aggregate secondary moods into a persistable string
                var uniqueMoods = SelectedSecondaryMoods.Distinct().ToList();
                CurrentEntry.SecondaryMoods = string.Join(",", uniqueMoods);

                await _journalService.SaveEntryAsync(CurrentEntry);

                // Sync the ID if this was a new insertion
                if (CurrentEntry.Id == 0)
                {
                    var savedEntry = await _journalService.GetEntryByDateAsync(CurrentEntry.EntryDate);
                    if (savedEntry != null)
                    {
                        CurrentEntry.Id = savedEntry.Id;
                    }
                }

                await Shell.Current.DisplayAlert("Saved", "Your journal entry has been captured.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Save failed: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Permanently removes the entry and its links from the database.
        /// </summary>
        [RelayCommand]
        public async Task DeleteAsync()
        {
            if (CurrentEntry == null || CurrentEntry.Id == 0) return;

            var confirm = await Shell.Current.DisplayAlert("Confirm Delete", "Are you absolutely sure?", "Delete", "Cancel");
            if (confirm)
            {
                await _journalService.DeleteEntryAsync(CurrentEntry);
                await LoadEntryArgs(SelectedDate); // Refresh the view
            }
        }

        /// <summary>
        /// Resets the current form to today's date and a blank state.
        /// </summary>
        [RelayCommand]
        public async Task NewEntry()
        {
            try
            {
                SelectedDate = DateTime.Today;
                var existing = await _journalService.GetEntryByDateAsync(SelectedDate);
                
                if (existing != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await Shell.Current.DisplayAlert("Notice", "You already have an entry for today. Editing current log.", "OK");
                    });
                    await LoadEntryArgs(SelectedDate);
                }
                else
                {
                    CurrentEntry = new JournalEntry { EntryDate = SelectedDate };
                    MarkdownText = string.Empty;
                    SelectedMood = null;
                    SelectedSecondaryMoods.Clear();
                    SelectedTags.Clear();
                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // --- Event Handlers & Helper Methods ---

        partial void OnMarkdownTextChanged(string value)
        {
            UpdatePreview(); // Reactive update for the rich-text preview
        }

        [RelayCommand]
        public void ClearMood()
        {
            SelectedMood = null;
            SelectedSecondaryMoods.Clear();
        }

        [RelayCommand]
        public void AddSecondaryFeeling()
        {
            if (string.IsNullOrWhiteSpace(NewSecondaryFeeling)) return;
            var text = NewSecondaryFeeling.Trim();

            if (!SelectedSecondaryMoods.Contains(text))
            {
                SelectedSecondaryMoods.Add(text);
            }
            NewSecondaryFeeling = string.Empty;
        }

        [RelayCommand]
        public void AddSecondaryFeelingFromSuggestion(Mood mood)
        {
            if (mood == null) return;
            if (!SelectedSecondaryMoods.Contains(mood.Name))
            {
                SelectedSecondaryMoods.Add(mood.Name);
            }
        }

        [RelayCommand]
        public void RemoveSecondaryFeeling(string feeling)
        {
            if (SelectedSecondaryMoods.Contains(feeling))
            {
                SelectedSecondaryMoods.Remove(feeling);
            }
        }

        // Standard palette for colorful UI tags
        private readonly List<string> _premiumColors = new() 
        { 
            "#6366F1", "#8B5CF6", "#EC4899", "#F43F5E", "#EF4444", 
            "#F97316", "#F59E0B", "#10B981", "#06B6D4", "#3B82F6" 
        };

        /// <summary>
        /// Adds a custom tag to the entry. Ensures case-insensitive uniqueness.
        /// </summary>
        [RelayCommand]
        public async Task AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagName)) return;

            var tagText = NewTagName.Trim();
            
            // Uniqueness Check
            if (SelectedTags.Any(t => t.Name.Equals(tagText, StringComparison.OrdinalIgnoreCase)))
            {
                await Shell.Current.DisplayAlert("Duplicate", "This tag is already added.", "OK");
                NewTagName = string.Empty;
                return;
            }

            // Length Validation
            if (tagText.Length > 20)
            {
                await Shell.Current.DisplayAlert("Limit Reached", "Tags must be short (max 20 chars).", "OK");
                return;
            }

            // To link a tag, the entry must exist in the DB first
            if (CurrentEntry.Id == 0)
            {
                await SaveAsync();
            }

            if (CurrentEntry.Id > 0)
            {
                var randomColor = _premiumColors[new Random().Next(_premiumColors.Count)];
                await _journalService.AddTagToEntryAsync(CurrentEntry.Id, tagText, randomColor);
                
                // Refresh tag list from DB
                var updatedTags = await _journalService.GetTagsForEntryAsync(CurrentEntry.Id);
                SelectedTags.Clear();
                foreach (var t in updatedTags) SelectedTags.Add(t);
            }

            NewTagName = string.Empty;
        }

        [RelayCommand]
        public async Task RemoveTag(Tag tag)
        {
            if (tag == null || CurrentEntry.Id == 0) return;

            await _journalService.RemoveTagFromEntryAsync(CurrentEntry.Id, tag.Id);
            SelectedTags.Remove(tag);
        }

        /// <summary>
        /// Uses the Markdig library to convert Markdown text into raw HTML for display in a WebView.
        /// </summary>
        private void UpdatePreview()
        {
            try 
            {
                if (string.IsNullOrWhiteSpace(MarkdownText))
                {
                    HtmlPreview = string.Empty;
                    return;
                }
                
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var result = Markdown.ToHtml(MarkdownText, pipeline);
                
                // Marshall UI update back to main thread
                MainThread.BeginInvokeOnMainThread(() => {
                    HtmlPreview = result;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Markdown Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles incoming shell navigation parameters (e.g. from the Timeline page).
        /// </summary>
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("date") && query["date"] is string dateStr)
            {
                if (DateTime.TryParse(dateStr, out var date))
                {
                    SelectedDate = date.Date;
                    await LoadEntryArgs(SelectedDate);
                }
            }
        }
    }
}
