using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;
using Markdig;
using System.Collections.ObjectModel;

namespace DailyJournalApp.ViewModels
{
    public partial class JournalViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly JournalService _journalService;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

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
        
        public ObservableCollection<Mood> Moods { get; } = new();
        // Changed to string to allow custom written feelings
        public ObservableCollection<string> SelectedSecondaryMoods { get; } = new();
        public ObservableCollection<Tag> SelectedTags { get; } = new();

        [ObservableProperty]
        private string newTagName = string.Empty;

        [ObservableProperty]
        private string newSecondaryFeeling = string.Empty;

        // Keep presets as "Suggestions"
        public ObservableCollection<Mood> MoodSuggestions { get; } = new();

        public JournalViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Journal";
            SelectedDate = DateTime.Today;
            // Initialize with an empty entry to avoid nulls before load
            CurrentEntry = new JournalEntry { EntryDate = DateTime.Today };
        }

        /// <summary>
        /// Loads the journal entry for the selected date and populates moods.
        /// </summary>
        [RelayCommand]
        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                // Always reload Moods to reflect potential database updates
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
                await Shell.Current.DisplayAlert("Error", $"Failed to load entry: {ex.Message}", "OK");
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
                    
                    // Load Tags
                    var tags = await _journalService.GetTagsForEntryAsync(entry.Id);
                    foreach (var t in tags) SelectedTags.Add(t);

                    // Load Secondary Moods (Deduplicate)
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
                    // New blank entry
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
        /// Saves the current journal entry to the database.
        /// </summary>
        [RelayCommand]
        public async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (CurrentEntry == null) return;

                if (CurrentEntry.Id == 0)
                {
                    var existing = await _journalService.GetEntryByDateAsync(CurrentEntry.EntryDate);
                    if (existing != null)
                    {
                        MainThread.BeginInvokeOnMainThread(async () => {
                            await Shell.Current.DisplayAlert("Journal Limit", "journal limit exceed(only one per day)", "OK");
                        });
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(CurrentEntry.Title))
                {
                    CurrentEntry.Title = "Untitled";
                }

                CurrentEntry.Content = MarkdownText;
                CurrentEntry.PrimaryMood = SelectedMood?.Name;
                
                // Save Secondary Moods (Deduplicate)
                var uniqueMoods = SelectedSecondaryMoods.Distinct().ToList();
                CurrentEntry.SecondaryMoods = string.Join(",", uniqueMoods);

                await _journalService.SaveEntryAsync(CurrentEntry);

                // Ensure Id is synced for new entries
                if (CurrentEntry.Id == 0)
                {
                    var savedEntry = await _journalService.GetEntryByDateAsync(CurrentEntry.EntryDate);
                    if (savedEntry != null)
                    {
                        CurrentEntry.Id = savedEntry.Id;
                    }
                }

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
                        await Shell.Current.DisplayAlert("Journal Limit", "journal limit exceed(only one per day)", "OK");
                    });
                }

                // Open a new blank journal entry page (not the saved one)
                CurrentEntry = new JournalEntry { EntryDate = SelectedDate };
                MarkdownText = string.Empty;
                SelectedMood = null;
                SelectedSecondaryMoods.Clear();
                SelectedTags.Clear();
                UpdatePreview();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to prepare new entry: {ex.Message}", "OK");
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

        // Premium Palette for Tags
        private readonly List<string> _premiumColors = new() 
        { 
            "#6366F1", "#8B5CF6", "#EC4899", "#F43F5E", "#EF4444", 
            "#F97316", "#F59E0B", "#10B981", "#06B6D4", "#3B82F6" 
        };

        /// <summary>
        /// Adds a new tag to the current journal entry with validation and logic for existing entries.
        /// </summary>
        [RelayCommand]
        public async Task AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagName)) return;

            var tagText = NewTagName.Trim();
            
            // Quality Check: Prevent duplicate tags (Case insensitive)
            if (SelectedTags.Any(t => t.Name.Equals(tagText, StringComparison.OrdinalIgnoreCase)))
            {
                await Shell.Current.DisplayAlert("Duplicate Tag", $"The tag '{tagText}' already exists for this entry.", "OK");
                NewTagName = string.Empty;
                return;
            }

            // Quality Check: Prevent excessively long tags
            if (tagText.Length > 20)
            {
                await Shell.Current.DisplayAlert("Invalid Tag", "Tags must be 20 characters or less.", "OK");
                return;
            }

            // For existing entries, persist immediately. For new, we'll save together.
            if (CurrentEntry.Id == 0)
            {
                // We need an ID to link tags, so save the entry first
                await SaveAsync();
            }

            if (CurrentEntry.Id > 0)
            {
                 // Assign a random premium color
                var randomColor = _premiumColors[new Random().Next(_premiumColors.Count)];

                await _journalService.AddTagToEntryAsync(CurrentEntry.Id, tagText, randomColor);
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
                
                // Ensure the output is updated on the main thread
                MainThread.BeginInvokeOnMainThread(() => {
                    HtmlPreview = result;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Markdown conversion error: {ex.Message}");
                HtmlPreview = $"<p style='color:red;'>Error rendering preview: {ex.Message}</p>";
            }
        }

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
