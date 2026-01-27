using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    public class JournalService
    {
        private readonly DatabaseService _databaseService;

        public JournalService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<JournalEntry> GetEntryByDateAsync(DateTime date)
        {
            var db = await _databaseService.GetConnectionAsync();
            // Compare Date component only. 
            // SQLite stores ticks or ISO strings depending on config, but usually we query by range or check equality logic.
            // Safe bet: Query all or use custom query. SQLite-net-pcl handles DateTime.
            // Let's optimize: WHERE EntryDate = date.Date
            
            // To ensure we match just the date part, we might need to be careful. 
            // Ideally EntryDate in DB is stored as Midnight.
            var targetDate = date.Date; 
            
            // Note: SQLite comparison might be tricky with ticks. 
            // Let's filter in memory if dataset is small, or use query. 
            // For now, let's try direct query assuming we save as Date.
            var entry = await db.Table<JournalEntry>()
                                .Where(e => e.EntryDate == targetDate)
                                .FirstOrDefaultAsync();
            return entry;
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            var db = await _databaseService.GetConnectionAsync();
            
            // Ensure ID is set if it exists? 
            // If ID is 0, it's an insert.
            // However, we also have a unique constraint on Date.
            // If we try to insert a new object for an existing date, it will fail.
            // so we should check if one exists first if ID is 0.
            
            if (entry.Id == 0)
            {
               var existing = await GetEntryByDateAsync(entry.EntryDate);
               if (existing != null)
               {
                   entry.Id = existing.Id;
                   entry.CreatedAt = existing.CreatedAt;
                   entry.UpdatedAt = DateTime.Now;
                   await db.UpdateAsync(entry);
               }
               else
               {
                   entry.CreatedAt = DateTime.Now;
                   entry.UpdatedAt = DateTime.Now;
                   await db.InsertAsync(entry);
               }
            }
            else
            {
                entry.UpdatedAt = DateTime.Now;
                await db.UpdateAsync(entry);
            }
        }

        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            var db = await _databaseService.GetConnectionAsync();
            await db.DeleteAsync(entry);
        }

        public async Task<List<Mood>> GetMoodsAsync()
        {
             var db = await _databaseService.GetConnectionAsync();
             return await db.Table<Mood>().ToListAsync();
        }
        
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<JournalEntry>().OrderByDescending(x => x.EntryDate).ToListAsync();
        }
    }
}
