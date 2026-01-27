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

        [ObservableProperty]
        private int currentStreak;

        [ObservableProperty]
        private string mostFrequentMood;

        [ObservableProperty]
        private int totalEntries;

        [ObservableProperty]
        private ObservableCollection<ISeries> weeklySeries = new();

        [ObservableProperty]
        private ObservableCollection<Axis> xAxes = new();

        [ObservableProperty]
        private string userName;

        private readonly SecurityService _securityService;

        public DashboardViewModel(JournalService journalService, SecurityService securityService)
        {
            _journalService = journalService;
            _securityService = securityService;
            Title = "Dashboard";
            UserIsAuthenticated = _securityService.IsAuthenticated;
        }

        [RelayCommand]
        public async Task LoadDashboardAsync()
        {
            UserName = Preferences.Default.Get("CurrentUser", "User");
            var entries = await _journalService.GetAllEntriesAsync();
            TotalEntries = entries.Count;

            if (entries.Any())
            {
                MostFrequentMood = entries.GroupBy(e => e.PrimaryMood)
                                          .OrderByDescending(g => g.Count())
                                          .First().Key ?? "None";
                
                CalculateStreak(entries);
                CalculateWeeklyOverview(entries);
            }
            else
            {
                MostFrequentMood = "None";
                CurrentStreak = 0;
                WeeklySeries.Clear();
            }
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
            int streak = 0;
            DateTime current = DateTime.Today;

            if (dates.Contains(current))
            {
                foreach (var date in dates)
                {
                    if (date == current)
                    {
                        streak++;
                        current = current.AddDays(-1);
                    }
                    else if (date < current)
                    {
                        break;
                    }
                }
            }
            CurrentStreak = streak;
        }
    }
}
