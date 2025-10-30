using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;

namespace SupertronicsRepairSystem.Services
{
    // Authentication service interface
    public interface IAuthService
    {
        // Sign in user
        Task<AuthResult> SignInAsync(string email, string password, bool rememberMe);

        // Register new user
        Task<AuthResult> SignUpAsync(string email, string password, string firstName, string surname, string phoneNumber, UserRole role);

        // Sign out current user
        Task SignOutAsync();

        // Get current user ID
        Task<string> GetCurrentUserIdAsync();

        // Get current user details
        Task<UserInfo> GetCurrentUserInfoAsync();

        // Check if user is authenticated
        Task<bool> IsAuthenticatedAsync();

        // Get Firebase ID token
        Task<string> GetIdTokenAsync();

        // Send password reset email
        Task<AuthResult> ResetPasswordAsync(string email);

        // Get all technicians from DB
        Task<List<UserInfo>> GetAllTechniciansAsync();

        // Get technician by ID from DB
        Task<UserInfo> GetTechnicianByIdAsync(string userId);

        // Update technician in DB and Firebase Auth
        Task<bool> UpdateTechnicianAsync(string userId, EditTechnicianViewModel model);

        // Delete technician from DB and Firebase Auth
        Task<bool> DeleteTechnicianAsync(string userId);
    }

    // User role types
    public enum UserRole
    {
        Customer,
        Technician,
        Owner
    }

    // Authentication result
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public UserRole? Role { get; set; }
        public string IdToken { get; set; }
    }

    // User information
    public class UserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }

        // Role checks
        public bool IsCustomer => Role == UserRole.Customer;
        public bool IsTechnician => Role == UserRole.Technician;
        public bool IsOwner => Role == UserRole.Owner;
    }
}