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

        public async Task<List<T>> GetAsync<T>() where T : new()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<T>().ToListAsync();
        }

        public async Task<JournalEntry> GetEntryByDateAsync(DateTime date)
        {
            var db = await _databaseService.GetConnectionAsync();
            var start = date.Date;
            var end = start.AddDays(1);
            
            var entry = await db.Table<JournalEntry>()
                                .Where(e => e.EntryDate >= start && e.EntryDate < end)
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
            
            // Delete associated tags first
            await db.Table<EntryTag>().Where(et => et.EntryId == entry.Id).DeleteAsync();
            
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

        /// <summary>
        /// Gets a paginated list of journal entries.
        /// </summary>
        /// <param name="pageIndex">0-based page index.</param>
        /// <param name="pageSize">Number of items per page.</param>
        public async Task<List<JournalEntry>> GetEntriesPaginatedAsync(int pageIndex, int pageSize)
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<JournalEntry>()
                            .OrderByDescending(x => x.EntryDate)
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        /// <summary>
        /// Gets the total number of journal entries in the database.
        /// </summary>
        public async Task<int> GetTotalEntriesCountAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<JournalEntry>().CountAsync();
        }

        #region Tag Management

        public async Task<List<Tag>> GetTagsAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<Tag>().ToListAsync();
        }

        public async Task<List<Tag>> GetTagsForEntryAsync(int entryId)
        {
            var db = await _databaseService.GetConnectionAsync();
            var query = "SELECT T.* FROM Tag T INNER JOIN EntryTag ET ON T.Id = ET.TagId WHERE ET.EntryId = ?";
            return await db.QueryAsync<Tag>(query, entryId);
        }

        // Efficient load for timeline
        public async Task<List<EntryTag>> GetAllEntryTagsAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<EntryTag>().ToListAsync();
        }

        public async Task AddTagToEntryAsync(int entryId, string tagName, string color)
        {
            var db = await _databaseService.GetConnectionAsync();
            
            // Create tag if it doesn't exist
            var tag = await db.Table<Tag>().Where(t => t.Name == tagName).FirstOrDefaultAsync();
            if (tag == null)
            {
                tag = new Tag { Name = tagName, HexColor = color }; 
                await db.InsertAsync(tag);
            }

            // check if link exists
            var existingLink = await db.Table<EntryTag>()
                                       .Where(et => et.EntryId == entryId && et.TagId == tag.Id)
                                       .FirstOrDefaultAsync();
            
            if (existingLink == null)
            {
                await db.InsertAsync(new EntryTag { EntryId = entryId, TagId = tag.Id });
            }
        }

        public async Task RemoveTagFromEntryAsync(int entryId, int tagId)
        {
            var db = await _databaseService.GetConnectionAsync();
            await db.Table<EntryTag>()
                    .Where(et => et.EntryId == entryId && et.TagId == tagId)
                    .DeleteAsync();
        }

        #endregion
    }
}
