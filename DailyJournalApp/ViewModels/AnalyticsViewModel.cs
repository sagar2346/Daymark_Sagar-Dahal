using CommunityToolkit.Mvvm.ComponentModel;
using DailyJournalApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace DailyJournalApp.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private ObservableCollection<ISeries> moodSeries = new();

        public AnalyticsViewModel(JournalService journalService)
        {
            _journalService = journalService;
            Title = "Analytics";
        }

        [RelayCommand]
        public async Task LoadAnalyticsAsync()
        {
            try
            {
                var entries = await _journalService.GetAllEntriesAsync();
                var moodCounts = entries.GroupBy(e => e.PrimaryMood)
                                        .Select(g => new { Mood = g.Key ?? "None", Count = (double)g.Count() });

                MoodSeries.Clear();
                foreach (var mc in moodCounts)
                {
                    MoodSeries.Add(new PieSeries<double>
                    {
                        Values = new[] { mc.Count },
                        Name = mc.Mood
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Analytics error: {ex.Message}");
            }
        }
    }
}
