using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.Services
{
    public interface ILoginService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<bool> CreateUserAsync(string username, string password);
        Task<bool> UserExistsAsync(string username);
        void ClearCurrentSession();
        string? CurrentUser { get; }
        int? CurrentUserId { get; }
        Task InitializeAsync();
    }

    public class LoginService(DatabaseContext context) : ILoginService
    {
        public string? CurrentUser {
            get;
            private set;
        }
        public int? CurrentUserId {
            get;
            private set;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            if ( string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) )
            {
                return false;
            }

            try
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if ( user == null )
                {
                    return false;
                }

                var hashedPassword = HashPassword(password, user.Salt);
                var isValid = user.PasswordHash == hashedPassword;

                if ( isValid )
                {
                    CurrentUser = username;
                    CurrentUserId = user.Id;
                    // Update last login time
                    user.LastLoginAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login validation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateUserAsync(string username, string password)
        {
            if ( string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) )
            {
                return false;
            }

            if ( password.Length < 6 )
            {
                return false; // Password too short
            }

            try
            {
                // Check if user already exists
                if ( await UserExistsAsync(username) )
                {
                    return false;
                }

                var salt = GenerateSalt();
                var hashedPassword = HashPassword(password, salt);

                var user = new UserModel {
                    Username = username,
                    PasswordHash = hashedPassword,
                    Salt = salt,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User creation error: {ex.Message}");
                return false;
            }
        }

        public Task<bool> UserExistsAsync(string username)
        {
            if ( string.IsNullOrWhiteSpace(username) )
            {
                return Task.FromResult(false);
            }

            try
            {
                return context.Users
                    .AnyAsync(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User exists check error: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public void ClearCurrentSession()
        {
            CurrentUser = null;
            CurrentUserId = null;
        }

        private static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
            var hashedBytes = sha256.ComputeHash(combinedBytes);
            return Convert.ToBase64String(hashedBytes);
        }

        public void Dispose()
        {
            context?.Dispose();
        }
    }
}