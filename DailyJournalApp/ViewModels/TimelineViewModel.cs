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

        [ObservableProperty]
        private DateTime? startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime? endDate = DateTime.Today;

        [ObservableProperty]
        private Mood? selectedMoodFilter;

        [ObservableProperty]
        private JournalEntry? selectedEntry;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayPageNumber))]
        private int currentPage = 0;

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

        public ObservableCollection<JournalEntry> Entries { get; } = new();
        public ObservableCollection<Mood> AvailableMoods { get; } = new();

        public TimelineViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Timeline";
            Initialize();
        }

        /// <summary>
        /// Initializes the timeline by loading moods, tags, and the first page of entries.
        /// </summary>
        [RelayCommand]
        public async Task Initialize()
        {
            IsBusy = true;
            // The rest of the initialization logic (e.g., loading entries)
            // will likely be moved here or called from here.
            // For now, we'll just set IsBusy and let LoadEntriesAsync handle the actual loading.
            // This method is likely intended to be called from the View's OnAppearing or similar.
            await LoadEntriesAsync();
            IsBusy = false;
        }

        partial void OnSearchTextChanged(string value) => FilterEntries();
        partial void OnStartDateChanged(DateTime? value) => FilterEntries();
        partial void OnEndDateChanged(DateTime? value) => FilterEntries();
        [ObservableProperty]
        private Tag? selectedTagFilter;

        public ObservableCollection<Tag> AvailableTags { get; } = new();

        partial void OnSelectedTagFilterChanged(Tag? value) => FilterEntries();

        /// <summary>
        /// Filters the loaded entries based on search text, date range, mood, and tags.
        /// </summary>
        private void FilterEntries()
        {
            try
            {
                // When filtering, we work with all entries locally as the dataset is presumed to be manageable for filtering once loaded.
                // However, the initial load is now paginated for performance.
                var filtered = _allEntries.AsEnumerable();

                // Search Text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(e => (e.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) || 
                                                  (e.Content?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // Date Range
                if (StartDate.HasValue)
                    filtered = filtered.Where(e => e.EntryDate.Date >= StartDate.Value.Date);
                
                if (EndDate.HasValue)
                    filtered = filtered.Where(e => e.EntryDate.Date <= EndDate.Value.Date);

                // Mood
                if (SelectedMoodFilter != null)
                    filtered = filtered.Where(e => e.PrimaryMood == SelectedMoodFilter.Name);

                // Tag [New]
                if (SelectedTagFilter != null)
                {
                    filtered = filtered.Where(e => e.Tags.Any(t => t.Id == SelectedTagFilter.Id));
                }

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
        /// Loads entries from the database with pagination support.
        /// </summary>
        [RelayCommand]
        public async Task LoadEntriesAsync()
        {
            try
            {
                IsBusy = true;
                
                var allMoods = await _journalService.GetMoodsAsync();
                if (!AvailableMoods.Any())
                {
                    foreach (var m in allMoods) AvailableMoods.Add(m);
                }

                var allTags = await _journalService.GetTagsAsync();
                AvailableTags.Clear();
                foreach (var t in allTags) AvailableTags.Add(t);

                // Pagination logic
                TotalCount = await _journalService.GetTotalEntriesCountAsync();
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
                
                CanGoPrevious = CurrentPage > 0;
                CanGoNext = CurrentPage < TotalPages - 1;

                _allEntries = await _journalService.GetEntriesPaginatedAsync(CurrentPage, PageSize);
                
                var entryTags = await _journalService.GetAllEntryTagsAsync();

                foreach (var entry in _allEntries)
                {
                    var moodData = allMoods.FirstOrDefault(m => m.Name == entry.PrimaryMood);
                    if (moodData != null)
                    {
                        entry.MoodEmoji = moodData.Emoji;
                        entry.MoodCategory = moodData.Category;
                        entry.MoodColor = moodData.Category switch
                        {
                            "Positive" => "#22C55E",
                            "Negative" => "#EF4444",
                            _ => "#10B981" // Success/Green instead of Blue
                        };
                    }

                    var linkedTagIds = entryTags.Where(et => et.EntryId == entry.Id).Select(et => et.TagId).ToList();
                    entry.Tags = allTags.Where(t => linkedTagIds.Contains(t.Id)).ToList();
                }

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

        /// <summary>
        /// Navigates to the next page of entries.
        /// </summary>
        [RelayCommand]
        public async Task NextPageAsync()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await LoadEntriesAsync();
            }
        }

        /// <summary>
        /// Navigates to the previous page of entries.
        /// </summary>
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

            bool confirm = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this entry?", "Yes", "No");
            if (confirm)
            {
                await _journalService.DeleteEntryAsync(entry);
                await LoadEntriesAsync();
            }
        }

        [RelayCommand]
        public async Task ViewEntry()
        {
            if (SelectedEntry == null) return;
            
            var dateStr = SelectedEntry.EntryDate.ToString("yyyy-MM-dd");
            await Shell.Current.GoToAsync($"//JournalPage?date={dateStr}");
            
            // Clear selection
            SelectedEntry = null;
        }

        [RelayCommand]
        public async Task GoToNewEntry()
        {
            var dateStr = DateTime.Today.ToString("yyyy-MM-dd");
            await Shell.Current.GoToAsync($"//JournalPage?date={dateStr}");
        }
    }
}
