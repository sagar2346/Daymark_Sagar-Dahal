using CommunityToolkit.Mvvm.ComponentModel;
using DailyJournalApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DailyJournalApp.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private ObservableCollection<ISeries> moodSeries = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> wordTrendSeries = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> timePatternSeries = new();
        
        [ObservableProperty]
        private ObservableCollection<ISeries> tagPopularitySeries = new();

        [ObservableProperty]
        private Axis[] xAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private string avgWordsPerEntry = "0";

        [ObservableProperty]
        private string totalWordsRecorded = "0";

        [ObservableProperty]
        private string peakJournalingTime = "Morning";
        
        [ObservableProperty]
        private string insightSummary = "Start writing to see your patterns!";

        [ObservableProperty]
        private string mostUsedTagPhrase = "No tags used yet.";

        public AnalyticsViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Insights & Analytics";
        }

        [RelayCommand]
        public async Task LoadAnalyticsAsync()
        {
            try
            {
                IsBusy = true;
                var entries = await _journalService.GetAllEntriesAsync();
                if (!entries.Any()) return;

                // 1. Mood Distribution (Pie)
                var moodCounts = entries.GroupBy(e => e.PrimaryMood ?? "Unknown")
                                        .Select(g => new { Mood = g.Key, Count = (double)g.Count() });

                MoodSeries.Clear();
                foreach (var mc in moodCounts)
                {
                    MoodSeries.Add(new PieSeries<double>
                    {
                        Values = new[] { mc.Count },
                        Name = mc.Mood,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        DataLabelsPaint = new SolidColorPaint(SKColors.White),
                        DataLabelsSize = 12,
                        DataLabelsFormatter = point => $"{point.Context.Series.Name}: {point.Coordinate.PrimaryValue}"
                    });
                }

                // 2. Word Count Trend (Line)
                var sortedEntries = entries.OrderBy(e => e.EntryDate).ToList();
                var wordCounts = sortedEntries.Select(e => (double)(e.Content?.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0)).ToArray();
                
                WordTrendSeries.Clear();
                WordTrendSeries.Add(new LineSeries<double>
                {
                    Values = wordCounts,
                    Name = "Words",
                    Stroke = new SolidColorPaint(SKColor.Parse("#818CF8")) { StrokeThickness = 4 },
                    GeometrySize = 12,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#818CF8")) { StrokeThickness = 2 },
                    Fill = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(new SKColor[] { SKColor.Parse("#818CF8").WithAlpha(40), SKColors.Transparent })
                });

                XAxes = new[] {
                    new Axis { 
                        Labels = sortedEntries.Select(e => e.EntryDate.ToString("MMM dd")).ToArray(),
                        LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                        SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1E293B")) { StrokeThickness = 1 }
                    }
                };

                // 3. Time Patterns (Bar)
                var timeGroups = entries.GroupBy(e => GetTimeSlot(e.CreatedAt))
                                       .Select(g => new { Slot = g.Key, Count = (double)g.Count() })
                                       .OrderBy(x => x.Slot)
                                       .ToList();

                TimePatternSeries.Clear();
                TimePatternSeries.Add(new ColumnSeries<double>
                {
                    Values = timeGroups.Select(x => x.Count).ToArray(),
                    Name = "Entries",
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColor.Parse("#6366F1")),
                    Rx = 8,
                    Ry = 8,
                    Padding = 10,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                });

                // 4. Tag Popularity (Bar)
                var entryTags = await _journalService.GetAllEntryTagsAsync();
                var tags = await _journalService.GetTagsAsync();
                
                var tagStats = entryTags.GroupBy(et => et.TagId)
                                        .Select(g => new { 
                                            Name = tags.FirstOrDefault(t => t.Id == g.Key)?.Name ?? "Unknown", 
                                            Count = (double)g.Count() 
                                        })
                                        .OrderByDescending(x => x.Count)
                                        .Take(5)
                                        .ToList();

                TagPopularitySeries.Clear();
                TagPopularitySeries.Add(new ColumnSeries<double>
                {
                    Values = tagStats.Select(x => x.Count).ToArray(),
                    Name = "Uses",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")), // Emerald
                    Rx = 8, Ry = 8,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8"))
                });

                // 5. Stats & Plain English Summaries
                TotalWordsRecorded = wordCounts.Sum().ToString("N0");
                AvgWordsPerEntry = wordCounts.Any() ? wordCounts.Average().ToString("F1") : "0";
                
                var topTime = timeGroups.OrderByDescending(x => x.Count).FirstOrDefault()?.Slot ?? "N/A";
                PeakJournalingTime = topTime;

                var topMood = moodCounts.OrderByDescending(x => x.Count).FirstOrDefault()?.Mood ?? "None";
                InsightSummary = $"You've been feeling mostly **{topMood}** lately. You tend to write most during the **{topTime}**.";
                
                var topTag = tagStats.FirstOrDefault();
                MostUsedTagPhrase = topTag != null 
                    ? $"Your go-to topic is **#{topTag.Name}**, which you've mentioned {topTag.Count} times!"
                    : "You haven't used many tags yet—try tagging your entries to see what matters most!";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Analytics error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetTimeSlot(DateTime dt)
        {
            int hour = dt.Hour;
            if (hour >= 5 && hour < 12) return "Morning";
            if (hour >= 12 && hour < 17) return "Afternoon";
            if (hour >= 17 && hour < 21) return "Evening";
            return "Night";
        }
    }
}
