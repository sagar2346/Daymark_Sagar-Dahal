using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyJournalApp.Models;
using DailyJournalApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DailyJournalApp.ViewModels
{
    /// <summary>
    /// ViewModel for the main Dashboard. 
    /// Aggregates data from across the app into user-friendly stats and visual charts.
    /// </summary>
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        private readonly AuthService _authService;

        // Statistics Properties
        [ObservableProperty]
        private int currentStreak;

        [ObservableProperty]
        private int longestStreak;

        [ObservableProperty]
        private string mostFrequentMood = string.Empty;

        [ObservableProperty]
        private int totalEntries;

        // Chart Data Properties (LiveCharts Integration)
        [ObservableProperty]
        private ObservableCollection<ISeries> weeklySeries = new();

        [ObservableProperty]
        private ObservableCollection<Axis> xAxes = new();

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string topTag = string.Empty;

        /// <summary>
        /// List of tags and their usage counts for display in a summary table.
        /// </summary>
        public ObservableCollection<TagCount> TagDistribution { get; } = new();

        public DashboardViewModel(JournalService journalService, AuthService authService)
        {
            _journalService = journalService;
            _authService = authService;
            Title = "Dashboard";
        }

        /// <summary>
        /// Main data loader for the dashboard.
        /// Recalculates all stats whenever the page is visited.
        /// </summary>
        [RelayCommand]
        public async Task LoadDashboardAsync()
        {
            UserName = _authService.GetCurrentUserName();
            var entries = await _journalService.GetAllEntriesAsync();
            TotalEntries = entries.Count;

            // Only perform complex math if there is data available
            if (entries.Any())
            {
                // Find mood mode (most frequent)
                MostFrequentMood = entries.GroupBy(e => e.PrimaryMood)
                                          .OrderByDescending(g => g.Count())
                                          .First().Key ?? "None";
                
                CalculateStreak(entries);
                CalculateWeeklyOverview(entries);
                await CalculateTagInsights();
            }
            else
            {
                // Reset to default empty state
                MostFrequentMood = "None";
                TopTag = "None";
                CurrentStreak = 0;
                WeeklySeries.Clear();
                TagDistribution.Clear();
            }
        }

        /// <summary>
        /// Analyzes tag popularity by joining Tags and EntryTags data.
        /// </summary>
        private async Task CalculateTagInsights()
        {
            var tags = await _journalService.GetTagsAsync();
            var entryTags = await _journalService.GetAsync<EntryTag>();

            if (!entryTags.Any()) 
            {
                TopTag = "None";
                TagDistribution.Clear();
                return;
            }

            // Aggregate counts using LINQ GroupBy
            var stats = entryTags.GroupBy(et => et.TagId)
                                 .Select(g => new TagCount 
                                 { 
                                     TagName = tags.FirstOrDefault(t => t.Id == g.Key)?.Name ?? "Unknown", 
                                     Count = g.Count() 
                                 })
                                 .OrderByDescending(x => x.Count)
                                 .ToList();

            TopTag = stats.FirstOrDefault()?.TagName ?? "None";
            
            TagDistribution.Clear();
            foreach (var s in stats.Take(5)) TagDistribution.Add(s);
        }

        /// <summary>
        /// Generates the data points for the 7-day bar chart on the UI.
        /// </summary>
        private void CalculateWeeklyOverview(List<JournalEntry> entries)
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            // Transform date list into frequency counts
            var counts = last7Days.Select(date => 
                (double)entries.Count(e => e.EntryDate.Date == date.Date))
                .ToArray();

            // Configure the Bar Chart series
            WeeklySeries.Clear();
            WeeklySeries.Add(new ColumnSeries<double>
            {
                Values = counts,
                Stroke = null,
                Padding = 2,
                Fill = new SolidColorPaint(SKColor.Parse("#0D9488")), // Teal Primary
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                MaxBarWidth = 35
            });

            // Configure the X-Axis labels (Mon, Tue, etc.)
            XAxes.Clear();
            XAxes.Add(new Axis
            {
                Labels = last7Days.Select(d => d.ToString("ddd")).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E2E8F0")) { StrokeThickness = 0 }
            });
        }

        /// <summary>
        /// Highly reactive streak algorithm.
        /// Calculates how many consecutive days the user has written.
        /// </summary>
        private void CalculateStreak(List<JournalEntry> entries)
        {
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().OrderByDescending(d => d).ToList();
            if (!dates.Any())
            {
                CurrentStreak = 0;
                LongestStreak = 0;
                return;
            }

            // Logic for Current Active Streak
            int current = 0;
            DateTime checkDate = DateTime.Today;
            
            // A streak is active if the user wrote today OR yesterday
            if (dates.Contains(checkDate) || dates.Contains(checkDate.AddDays(-1)))
            {
                // Start checking from the most recent entry
                if (!dates.Contains(checkDate)) checkDate = checkDate.AddDays(-1);

                foreach (var date in dates)
                {
                    if (date == checkDate)
                    {
                        current++;
                        checkDate = checkDate.AddDays(-1); // Go back one day and repeat
                    }
                    else if (date < checkDate) break; // Gap found, streak over
                }
            }
            CurrentStreak = current;

            // Logic for All-Time Longest Streak
            int max = 0;
            int temp = 0;
            DateTime? prev = null;
            
            // Ascending order to find the longest sequence of adjacent days
            var ascendingDates = dates.OrderBy(d => d).ToList();
            foreach (var date in ascendingDates)
            {
                if (prev == null || date == prev.Value.AddDays(1))
                {
                    temp++;
                }
                else
                {
                    max = Math.Max(max, temp);
                    temp = 1;
                }
                prev = date;
            }
            LongestStreak = Math.Max(max, temp);
        }
    }
}
