using SQLite;
using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    /// <summary>
    /// Core data persistence service. Manages the SQLite connection, 
    /// table initialization, and generic CRUD (Create, Read, Update, Delete) operations.
    /// </summary>
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        
        // Lock to prevent multiple threads from initializing the connection at the exact same time
        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public DatabaseService()
        {
        }

        /// <summary>
        /// Initializes the database connection and creates tables if they don't exist.
        /// This method is thread-safe and lazy-loaded.
        /// </summary>
        private async Task Init()
        {
            if (_database is not null)
                return;

            await _initializationSemaphore.WaitAsync();
            try
            {
                // Double-check null after obtaining lock
                if (_database is not null)
                    return;

                var dbPath = Constants.DatabasePath;
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);
                
                // Ensure the local file directory exists
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Create Application Tables
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<JournalEntry>();
                await _database.CreateTableAsync<Mood>();
                await _database.CreateTableAsync<Tag>();
                await _database.CreateTableAsync<EntryTag>();

                // Run data seeding in a background task to keep the main UI thread responsive
                _ = Task.Run(async () => {
                    try {
                        await SeedDataAsync();
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Seed error: {ex.Message}");
                    }
                });
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        /// <summary>
        /// Populates the database with default mood data if empty or corrupted.
        /// </summary>
        private async Task SeedDataAsync()
        {
            var moodsInDb = await _database.Table<Mood>().ToListAsync();
            
            // If any mood is missing an emoji, or if count is wrong, reset and re-seed all
            if (moodsInDb.Count != 8 || moodsInDb.Any(m => string.IsNullOrEmpty(m.Emoji)))
            {
                await _database.DeleteAllAsync<Mood>();
                var moods = new List<Mood>
                {
                    new Mood { Name = "Happy", Category = "Positive", Emoji = "😊" },
                    new Mood { Name = "Excited", Category = "Positive", Emoji = "🤩" },
                    new Mood { Name = "Grateful", Category = "Positive", Emoji = "🙏" },
                    new Mood { Name = "Calm", Category = "Neutral", Emoji = "🧘" },
                    new Mood { Name = "Neutral", Category = "Neutral", Emoji = "😐" },
                    new Mood { Name = "Tired", Category = "Negative", Emoji = "🥱" },
                    new Mood { Name = "Sad", Category = "Negative", Emoji = "☹️" },
                    new Mood { Name = "Anxious", Category = "Negative", Emoji = "😰" }
                };
                await _database.InsertAllAsync(moods);
            }
        }

        /// <summary>
        /// Returns the active SQLite connection, initializing it if necessary.
        /// </summary>
        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await Init();
            return _database;
        }

        /// <summary>
        /// Retrieves all items of a specified type from the database.
        /// </summary>
        public async Task<List<T>> GetAsync<T>() where T : new()
        {
            await Init();
            return await _database.Table<T>().ToListAsync();
        }

        /// <summary>
        /// Saves an item by either inserting it or replacing an existing record by Primary Key.
        /// </summary>
        public async Task<int> SaveAsync<T>(T item) where T : new()
        {
            await Init();
            return await _database.InsertOrReplaceAsync(item);
        }
        
        /// <summary>
        /// Inserts a new record into the specified table.
        /// </summary>
        public async Task<int> InsertAsync<T>(T item) where T : new()
        {
             await Init();
             return await _database.InsertAsync(item);
        }

        /// <summary>
        /// Updates an existing record in the specified table.
        /// </summary>
        public async Task<int> UpdateAsync<T>(T item) where T : new()
        {
            await Init();
            return await _database.UpdateAsync(item);
        }

        /// <summary>
        /// Deletes a record from the database based on its Primary Key.
        /// </summary>
        public async Task<int> DeleteAsync<T>(object pk) where T : new()
        {
            await Init();
            return await _database.DeleteAsync<T>(pk);
        }
    }
}
