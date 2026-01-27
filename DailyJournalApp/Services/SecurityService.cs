using System.Security.Cryptography;
using System.Text;
using DailyJournalApp.Models;
using Microsoft.Maui.Storage;

namespace DailyJournalApp.Services
{
    public class SecurityService
    {
        private readonly DatabaseService _databaseService;
        public bool IsAuthenticated 
        { 
            get => Preferences.Default.Get("SessionAuthenticated", false); 
            private set => Preferences.Default.Set("SessionAuthenticated", value); 
        }

        public SecurityService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> IsSignedUpAsync()
        {
            var user = await GetUserAsync();
            return user != null && !string.IsNullOrEmpty(user.PasswordHash);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await GetUserAsync();
            if (user == null) return false;

            var inputHash = HashText(password);
            if (user.Username == username && user.PasswordHash == inputHash)
            {
                IsAuthenticated = true;
                Preferences.Default.Set("CurrentUser", username);
                return true;
            }
            return false;
        }

        public async Task SignupAsync(string username, string password)
        {
            var hash = HashText(password);
            var user = await GetUserAsync();
            if (user == null)
            {
                user = new User 
                { 
                    Username = username, 
                    PasswordHash = hash,
                    CreatedAt = DateTime.Now
                };
                await _databaseService.InsertAsync(user);
            }
            else
            {
                user.Username = username;
                user.PasswordHash = hash;
                await _databaseService.UpdateAsync(user);
            }
            IsAuthenticated = true;
            Preferences.Default.Set("CurrentUser", username);
        }

        public async Task UpdatePasswordAsync(string newPassword)
        {
            var hash = HashText(newPassword);
            var user = await GetUserAsync();
            if (user != null)
            {
                user.PasswordHash = hash;
                await _databaseService.UpdateAsync(user);
            }
        }

        public void Logout()
        {
            IsAuthenticated = false;
            Preferences.Default.Remove("CurrentUser");
        }

        public async Task<User> GetUserAsync()
        {
             var db = await _databaseService.GetConnectionAsync();
             return await db.Table<User>().FirstOrDefaultAsync();
        }

        private string HashText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
