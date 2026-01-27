using SQLite;

namespace DailyJournalApp.Models
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string? Name { get; set; }

        public string? HexColor { get; set; }
    }
}
