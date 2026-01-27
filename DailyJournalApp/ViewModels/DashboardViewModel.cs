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
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private int currentStreak;

        [ObservableProperty]
        private int longestStreak;

        [ObservableProperty]
        private string mostFrequentMood = string.Empty;

        [ObservableProperty]
        private int totalEntries;

        [ObservableProperty]
        private ObservableCollection<ISeries> weeklySeries = new();

        [ObservableProperty]
        private ObservableCollection<Axis> xAxes = new();

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string topTag = string.Empty;

        public ObservableCollection<TagCount> TagDistribution { get; } = new();

        public DashboardViewModel(JournalService journalService, AuthService authService)
        {
            _journalService = journalService;
            _authService = authService;
            Title = "Dashboard";
        }

        [RelayCommand]
        public async Task LoadDashboardAsync()
        {
            UserName = _authService.GetCurrentUserName();
            var entries = await _journalService.GetAllEntriesAsync();
            TotalEntries = entries.Count;

            if (entries.Any())
            {
                MostFrequentMood = entries.GroupBy(e => e.PrimaryMood)
                                          .OrderByDescending(g => g.Count())
                                          .First().Key ?? "None";
                
                CalculateStreak(entries);
                CalculateWeeklyOverview(entries);
                await CalculateTagInsights();
            }
            else
            {
                MostFrequentMood = "None";
                TopTag = "None";
                CurrentStreak = 0;
                WeeklySeries.Clear();
                TagDistribution.Clear();
            }
        }

        private async Task CalculateTagInsights()
        {
            var tags = await _journalService.GetTagsAsync();
            var entryTags = await _journalService.GetAsync<EntryTag>(); // Generic fetch if available

            if (!entryTags.Any()) 
            {
                TopTag = "None";
                TagDistribution.Clear();
                return;
            }

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

        private void CalculateWeeklyOverview(List<JournalEntry> entries)
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var counts = last7Days.Select(date => 
                (double)entries.Count(e => e.EntryDate.Date == date.Date))
                .ToArray();

            WeeklySeries.Clear();
            WeeklySeries.Add(new ColumnSeries<double>
            {
                Values = counts,
                Stroke = null,
                Padding = 2,
                Fill = new SolidColorPaint(SKColor.Parse("#0D9488")), // Primary Teal
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                MaxBarWidth = 35
            });

            XAxes.Clear();
            XAxes.Add(new Axis
            {
                Labels = last7Days.Select(d => d.ToString("ddd")).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E2E8F0")) { StrokeThickness = 0 }
            });
        }

        private void CalculateStreak(List<JournalEntry> entries)
        {
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().OrderByDescending(d => d).ToList();
            if (!dates.Any())
            {
                CurrentStreak = 0;
                LongestStreak = 0;
                return;
            }

            // Current Streak
            int current = 0;
            DateTime checkDate = DateTime.Today;
            if (dates.Contains(checkDate) || dates.Contains(checkDate.AddDays(-1)))
            {
                // If they haven't written today but did yesterday, the streak is still alive until they miss today completely.
                // However, usually streak = days up to yesterday if today is missing.
                // Let's be strict: if today is missing, streak = days up to yesterday.
                if (!dates.Contains(checkDate)) checkDate = checkDate.AddDays(-1);

                foreach (var date in dates)
                {
                    if (date == checkDate)
                    {
                        current++;
                        checkDate = checkDate.AddDays(-1);
                    }
                    else if (date < checkDate) break;
                }
            }
            CurrentStreak = current;

            // Longest Streak
            int max = 0;
            int temp = 0;
            DateTime? prev = null;
            
            // Order ascending to find longest gap-less sequence
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
