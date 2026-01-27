using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    /// <summary>
    /// Business logic service for managing journal entries, moods, and tags.
    /// Orchestrates complex data operations between the UI and the Database service.
    /// </summary>
    public class JournalService
    {
        private readonly DatabaseService _databaseService;

        public JournalService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Generic fetch for any model type.
        /// </summary>
        public async Task<List<T>> GetAsync<T>() where T : new()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<T>().ToListAsync();
        }

        /// <summary>
        /// Retrieves a single journal entry based on a specific date.
        /// Useful for the "one-entry-per-day" system.
        /// </summary>
        public async Task<JournalEntry> GetEntryByDateAsync(DateTime date)
        {
            var db = await _databaseService.GetConnectionAsync();
            var start = date.Date;
            var end = start.AddDays(1);
            
            // Filter by 24-hour range for the selected day
            var entry = await db.Table<JournalEntry>()
                                .Where(e => e.EntryDate >= start && e.EntryDate < end)
                                .FirstOrDefaultAsync();
            return entry;
        }

        /// <summary>
        /// Sophisticated save logic that handles both new entries and updates.
        /// Automatically checks for existing entries by date to prevent data duplication.
        /// </summary>
        public async Task SaveEntryAsync(JournalEntry entry)
        {
            var db = await _databaseService.GetConnectionAsync();
            
            // Logic for New Entries (ID is 0)
            if (entry.Id == 0)
            {
               // Quality Check: Ensure we don't accidentally insert a duplicate date
               var existing = await GetEntryByDateAsync(entry.EntryDate);
               if (existing != null)
               {
                   // Convert to update if record exists
                   entry.Id = existing.Id;
                   entry.CreatedAt = existing.CreatedAt;
                   entry.UpdatedAt = DateTime.Now;
                   await db.UpdateAsync(entry);
               }
               else
               {
                   // Perform clean insert
                   entry.CreatedAt = DateTime.Now;
                   entry.UpdatedAt = DateTime.Now;
                   await db.InsertAsync(entry);
               }
            }
            else
            {
                // Simple Update for existing entries
                entry.UpdatedAt = DateTime.Now;
                await db.UpdateAsync(entry);
            }
        }

        /// <summary>
        /// Deletes an entry and cleans up associated metadata (EntryTags) to maintain database integrity.
        /// </summary>
        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            var db = await _databaseService.GetConnectionAsync();
            
            // Referential Integrity: Delete associated tags first
            await db.Table<EntryTag>().Where(et => et.EntryId == entry.Id).DeleteAsync();
            
            await db.DeleteAsync(entry);
        }

        /// <summary>
        /// Returns all pre-configured mood objects from the database.
        /// </summary>
        public async Task<List<Mood>> GetMoodsAsync()
        {
             var db = await _databaseService.GetConnectionAsync();
             return await db.Table<Mood>().ToListAsync();
        }
        
        /// <summary>
        /// Returns all journal entries sorted by newest first.
        /// </summary>
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<JournalEntry>().OrderByDescending(x => x.EntryDate).ToListAsync();
        }

        /// <summary>
        /// High-performance paginated retrieval for the Timeline view.
        /// </summary>
        /// <param name="pageIndex">0-based page number.</param>
        /// <param name="pageSize">Amount of records to return.</param>
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
        /// Gets the total global count of entries for pagination math.
        /// </summary>
        public async Task<int> GetTotalEntriesCountAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<JournalEntry>().CountAsync();
        }

        #region Tag Management

        /// <summary>
        /// Returns a full list of all unique tags used across the application.
        /// </summary>
        public async Task<List<Tag>> GetTagsAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<Tag>().ToListAsync();
        }

        /// <summary>
        /// Uses an optimized SQL Join query to find all tags linked to a specific entry.
        /// </summary>
        public async Task<List<Tag>> GetTagsForEntryAsync(int entryId)
        {
            var db = await _databaseService.GetConnectionAsync();
            var query = "SELECT T.* FROM Tag T INNER JOIN EntryTag ET ON T.Id = ET.TagId WHERE ET.EntryId = ?";
            return await db.QueryAsync<Tag>(query, entryId);
        }

        /// <summary>
        /// Retrieves all entry-tag links (Used for efficient filtering in the Timeline ViewModel).
        /// </summary>
        public async Task<List<EntryTag>> GetAllEntryTagsAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            return await db.Table<EntryTag>().ToListAsync();
        }

        /// <summary>
        /// Links a tag to an entry. Automatically creates the tag if it's new.
        /// </summary>
        public async Task AddTagToEntryAsync(int entryId, string tagName, string color)
        {
            var db = await _databaseService.GetConnectionAsync();
            
            // Check for Tag Existence
            var tag = await db.Table<Tag>().Where(t => t.Name == tagName).FirstOrDefaultAsync();
            if (tag == null)
            {
                tag = new Tag { Name = tagName, HexColor = color }; 
                await db.InsertAsync(tag);
            }

            // check if relationship already exists
            var existingLink = await db.Table<EntryTag>()
                                       .Where(et => et.EntryId == entryId && et.TagId == tag.Id)
                                       .FirstOrDefaultAsync();
            
            if (existingLink == null)
            {
                await db.InsertAsync(new EntryTag { EntryId = entryId, TagId = tag.Id });
            }
        }

        /// <summary>
        /// Unlinks a tag from an entry.
        /// </summary>
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
