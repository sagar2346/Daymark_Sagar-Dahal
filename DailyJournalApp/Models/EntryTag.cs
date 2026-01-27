using SQLite;

namespace DailyJournalApp.Models
{
    public class EntryTag
    {
        [Indexed]
        public int EntryId { get; set; }

        [Indexed]
        public int TagId { get; set; }
    }
}
