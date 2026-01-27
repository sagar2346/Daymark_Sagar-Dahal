using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;

namespace DailyJournalApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Timeline Page. 
    /// Manages a searchable, filterable, and paginated list of all historical journal entries.
    /// </summary>
    public partial class TimelineViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        
        // Comprehensive local mirror of current page entries for filtering
        private List<JournalEntry> _allEntries = new();

        // --- Search and Filter State ---

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private DateTime? startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime? endDate = DateTime.Today;

        [ObservableProperty]
        private Mood? selectedMoodFilter;

        [ObservableProperty]
        private Tag? selectedTagFilter;

        [ObservableProperty]
        private JournalEntry? selectedEntry;

        // --- Pagination State ---

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayPageNumber))]
        private int currentPage = 0;

        /// <summary>
        /// Human-readable page number (1-based index).
        /// </summary>
        public int DisplayPageNumber => CurrentPage + 1;

        [ObservableProperty]
        private int pageSize = 5;

        [ObservableProperty]
        private int totalCount;

        [ObservableProperty]
        private int totalPages;

        [ObservableProperty]
        private bool canGoNext;

        [ObservableProperty]
        private bool canGoPrevious;

        // --- Collections ---
        public ObservableCollection<JournalEntry> Entries { get; } = new();
        public ObservableCollection<Mood> AvailableMoods { get; } = new();
        public ObservableCollection<Tag> AvailableTags { get; } = new();

        public TimelineViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Timeline";
            
            // Initializing will load the first batch of data
            Initialize();
        }

        /// <summary>
        /// Entry point for the timeline view. 
        /// Ensures all metadata is ready before loading the paginated dataset.
        /// </summary>
        [RelayCommand]
        public async Task Initialize()
        {
            IsBusy = true;
            await LoadEntriesAsync();
            IsBusy = false;
        }

        // --- Reactive Property Triggers (Auto-filter on change) ---
        partial void OnSearchTextChanged(string value) => FilterEntries();
        partial void OnStartDateChanged(DateTime? value) => FilterEntries();
        partial void OnEndDateChanged(DateTime? value) => FilterEntries();
        partial void OnSelectedTagFilterChanged(Tag? value) => FilterEntries();

        /// <summary>
        /// Core filtering engine. 
        /// Processes the current page of entries against multiple UI filter parameters.
        /// </summary>
        private void FilterEntries()
        {
            try
            {
                var filtered = _allEntries.AsEnumerable();

                // 1. Text Search (Matches Title or Body Content)
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(e => (e.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) || 
                                                  (e.Content?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // 2. Temporal Filters
                if (StartDate.HasValue)
                    filtered = filtered.Where(e => e.EntryDate.Date >= StartDate.Value.Date);
                
                if (EndDate.HasValue)
                    filtered = filtered.Where(e => e.EntryDate.Date <= EndDate.Value.Date);

                // 3. Mood Categorization Filter
                if (SelectedMoodFilter != null)
                    filtered = filtered.Where(e => e.PrimaryMood == SelectedMoodFilter.Name);

                // 4. Tag Association Filter
                if (SelectedTagFilter != null)
                {
                    filtered = filtered.Where(e => e.Tags.Any(t => t.Id == SelectedTagFilter.Id));
                }

                // Hydrate the UI collection
                Entries.Clear();
                foreach (var entry in filtered.ToList())
                {
                    Entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Filtering error: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches a specific window of data from the database.
        /// Performs automatic 'hydration' by mapping raw data to UI-friendly emojis and colors.
        /// </summary>
        [RelayCommand]
        public async Task LoadEntriesAsync()
        {
            try
            {
                IsBusy = true;
                
                // Fetch Global Metadata
                var allMoods = await _journalService.GetMoodsAsync();
                if (!AvailableMoods.Any())
                {
                    foreach (var m in allMoods) AvailableMoods.Add(m);
                }

                var allTags = await _journalService.GetTagsAsync();
                AvailableTags.Clear();
                foreach (var t in allTags) AvailableTags.Add(t);

                // Calculate Pagination Bounds
                TotalCount = await _journalService.GetTotalEntriesCountAsync();
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                
                CanGoPrevious = CurrentPage > 0;
                CanGoNext = CurrentPage < TotalPages - 1;

                // Load Data Segment
                _allEntries = await _journalService.GetEntriesPaginatedAsync(CurrentPage, PageSize);
                
                // Data Hydration: Link tags and mood metadata for visual display
                var entryTags = await _journalService.GetAllEntryTagsAsync();

                foreach (var entry in _allEntries)
                {
                    // Map Mood Properties
                    var moodData = allMoods.FirstOrDefault(m => m.Name == entry.PrimaryMood);
                    if (moodData != null)
                    {
                        entry.MoodEmoji = moodData.Emoji;
                        entry.MoodCategory = moodData.Category;
                        entry.MoodColor = moodData.Category switch
                        {
                            "Positive" => "#22C55E", // Emerald Green
                            "Negative" => "#EF4444", // Rose Red
                            _ => "#10B981"          // Default Teal
                        };
                    }

                    // Map Tag Properties for the 'Tags' chip display
                    var linkedTagIds = entryTags.Where(et => et.EntryId == entry.Id).Select(et => et.TagId).ToList();
                    entry.Tags = allTags.Where(t => linkedTagIds.Contains(t.Id)).ToList();
                }

                FilterEntries();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Load Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- Navigation Commands ---

        [RelayCommand]
        public async Task NextPageAsync()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await LoadEntriesAsync();
            }
        }

        [RelayCommand]
        public async Task PreviousPageAsync()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                await LoadEntriesAsync();
            }
        }

        [RelayCommand]
        public void ClearFilters()
        {
            SearchText = string.Empty;
            StartDate = null;
            EndDate = null;
            SelectedMoodFilter = null;
            SelectedTagFilter = null;
            FilterEntries();
        }

        [RelayCommand]
        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            if (entry == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Action Required", "Delete this journal entry permanently?", "Delete", "Cancel");
            if (confirm)
            {
                await _journalService.DeleteEntryAsync(entry);
                await LoadEntriesAsync();
            }
        }

        /// <summary>
        /// Navigates to the Journal editing page for a specific entry.
        /// </summary>
        [RelayCommand]
        public async Task ViewEntry()
        {
            if (SelectedEntry == null) return;
            
            var dateStr = SelectedEntry.EntryDate.ToString("yyyy-MM-dd");
            await Shell.Current.GoToAsync($"//JournalPage?date={dateStr}");
            
            SelectedEntry = null; // Clear selection
        }

        /// <summary>
        /// Global shortcut to start a new entry for today.
        /// </summary>
        [RelayCommand]
        public async Task GoToNewEntry()
        {
            var dateStr = DateTime.Today.ToString("yyyy-MM-dd");
            await Shell.Current.GoToAsync($"//JournalPage?date={dateStr}");
        }
    }
}
