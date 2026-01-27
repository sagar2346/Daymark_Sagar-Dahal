using System.Security.Cryptography;
using System.Text;

namespace DailyJournalApp.Services
{
    /// <summary>
    /// Provides authentication and session management services.
    /// Uses SHA256 hashing for password security and Maui Preferences for session persistence.
    /// </summary>
    public class AuthService
    {
        // Global keys for local device storage (Preferences)
        private const string PasswordKey = "UserPasswordHash";
        private const string IsAuthenticatedKey = "IsLoggedIn";
        private const string UserNameKey = "CurrentUserName";

        /// <summary>
        /// Checks if the user has already configured a password during initial setup.
        /// </summary>
        public bool IsPasswordSet()
        {
            return Preferences.Default.ContainsKey(PasswordKey);
        }

        /// <summary>
        /// Determines if there is currently an active, authenticated session.
        /// </summary>
        public bool IsAuthenticated()
        {
            return Preferences.Default.Get(IsAuthenticatedKey, false);
        }

        /// <summary>
        /// Retrieves the personal name of the user for UI greetings.
        /// </summary>
        public string GetCurrentUserName()
        {
            return Preferences.Default.Get(UserNameKey, "User");
        }

        /// <summary>
        /// Saves or updates the user's display name.
        /// </summary>
        public void SetCurrentUserName(string name)
        {
            Preferences.Default.Set(UserNameKey, name);
        }

        /// <summary>
        /// Hashes and securely stores a new password on the device.
        /// </summary>
        public void SetPassword(string password)
        {
            string hash = HashPassword(password);
            Preferences.Default.Set(PasswordKey, hash);
        }

        /// <summary>
        /// Verifies a password attempt by comparing SHA256 hashes.
        /// If valid, initializes an authenticated session.
        /// </summary>
        public bool VerifyPassword(string password)
        {
            // Safety check: fail immediately if no password has ever been set
            if (!IsPasswordSet()) return false;

            string storedHash = Preferences.Default.Get(PasswordKey, string.Empty);
            string inputHash = HashPassword(password);

            // Compare calculated hash against stored hash for security
            bool isValid = storedHash == inputHash;
            if (isValid)
            {
                Preferences.Default.Set(IsAuthenticatedKey, true);
            }
            return isValid;
        }

        /// <summary>
        /// Terminates the current authenticated session.
        /// </summary>
        public void Logout()
        {
            Preferences.Default.Set(IsAuthenticatedKey, false);
        }

        /// <summary>
        /// Internal helper to transform raw strings into secure Base64 hashes.
        /// </summary>
        private string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
