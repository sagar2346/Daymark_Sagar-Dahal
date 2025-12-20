using SQLite;

namespace DailyJournalApp.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime EntryDate { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string PrimaryMood { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
