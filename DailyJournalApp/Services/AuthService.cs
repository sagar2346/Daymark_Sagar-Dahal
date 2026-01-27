using System.Security.Cryptography;
using System.Text;

namespace DailyJournalApp.Services
{
    public class AuthService
    {
        private const string PasswordKey = "UserPasswordHash";
        private const string IsAuthenticatedKey = "IsLoggedIn";
        private const string UserNameKey = "CurrentUserName";

        public bool IsPasswordSet()
        {
            return Preferences.Default.ContainsKey(PasswordKey);
        }

        public bool IsAuthenticated()
        {
            return Preferences.Default.Get(IsAuthenticatedKey, false);
        }

        public string GetCurrentUserName()
        {
            return Preferences.Default.Get(UserNameKey, "User");
        }

        public void SetCurrentUserName(string name)
        {
            Preferences.Default.Set(UserNameKey, name);
        }

        public void SetPassword(string password)
        {
            string hash = HashPassword(password);
            Preferences.Default.Set(PasswordKey, hash);
        }

        public bool VerifyPassword(string password)
        {
            if (!IsPasswordSet()) return false;

            string storedHash = Preferences.Default.Get(PasswordKey, string.Empty);
            string inputHash = HashPassword(password);

            bool isValid = storedHash == inputHash;
            if (isValid)
            {
                Preferences.Default.Set(IsAuthenticatedKey, true);
            }
            return isValid;
        }

        public void Logout()
        {
            Preferences.Default.Set(IsAuthenticatedKey, false);
        }

        private string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
