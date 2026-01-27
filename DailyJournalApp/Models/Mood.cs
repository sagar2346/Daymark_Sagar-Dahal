using SQLite;

namespace DailyJournalApp.Models
{
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Category { get; set; } // Positive, Neutral, Negative
        public string? Icon { get; set; }
        public string? Emoji { get; set; }
    }
}
