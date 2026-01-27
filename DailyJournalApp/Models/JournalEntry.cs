using SQLite;

namespace DailyJournalApp.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public DateTime EntryDate { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        // Storing Mood Name or Id. Storing Name for simplicity as per requirement "PrimaryMood string"
        public string PrimaryMood { get; set; }

        public string SecondaryMoods { get; set; } // Comma separated if needed, or join table

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
