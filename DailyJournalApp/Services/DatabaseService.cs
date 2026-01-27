using SQLite;
using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public DatabaseService()
        {
        }

        private async Task Init()
        {
            if (_database is not null)
                return;

            await _initializationSemaphore.WaitAsync();
            try
            {
                if (_database is not null)
                    return;

                var dbPath = Constants.DatabasePath;
                _database = new SQLiteAsyncConnection(dbPath, Constants.Flags);
                
                // Ensure the directory exists
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Create tables
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<JournalEntry>();
                await _database.CreateTableAsync<Mood>();
                await _database.CreateTableAsync<Tag>();
                await _database.CreateTableAsync<EntryTag>();

                // Run seeding in background to avoid blocking initial UI if it's heavy
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

        private async Task SeedDataAsync()
        {
            var moodsInDb = await _database.Table<Mood>().ToListAsync();
            
            // If any mood is missing an emoji, or if count is wrong, re-seed all
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

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await Init();
            return _database;
        }

        // Generic methods for simple CRUD if needed, or expose connection
        public async Task<List<T>> GetAsync<T>() where T : new()
        {
            await Init();
            return await _database.Table<T>().ToListAsync();
        }

        public async Task<int> SaveAsync<T>(T item) where T : new()
        {
            await Init();
            return await _database.InsertOrReplaceAsync(item); // Insert or Update
        }
        
        public async Task<int> InsertAsync<T>(T item) where T : new()
        {
             await Init();
             return await _database.InsertAsync(item);
        }

        public async Task<int> UpdateAsync<T>(T item) where T : new()
        {
            await Init();
            return await _database.UpdateAsync(item);
        }

        public async Task<int> DeleteAsync<T>(object pk) where T : new()
        {
            await Init();
            return await _database.DeleteAsync<T>(pk);
        }
    }
}
