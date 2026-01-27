using SQLite;

namespace DailyJournalApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PinHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
