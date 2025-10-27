using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;

namespace SupertronicsRepairSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(string email, string password, bool rememberMe);
        Task<AuthResult> SignUpAsync(string email, string password, string firstName, string surname, string phoneNumber, UserRole role);
        Task SignOutAsync();
        Task<string> GetCurrentUserIdAsync();
        Task<UserInfo> GetCurrentUserInfoAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetIdTokenAsync();
        Task<AuthResult> ResetPasswordAsync(string email);
        Task<List<UserInfo>> GetAllTechniciansAsync();
        Task<UserInfo> GetTechnicianByIdAsync(string userId);
        Task<bool> UpdateTechnicianAsync(string userId, EditTechnicianViewModel model);
        Task<bool> DeleteTechnicianAsync(string userId);
    }

    public enum UserRole
    {
        Customer,
        Technician,
        Owner
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public UserRole? Role { get; set; }
        public string IdToken { get; set; }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }

        public bool IsCustomer => Role == UserRole.Customer;
        public bool IsTechnician => Role == UserRole.Technician;
        public bool IsOwner => Role == UserRole.Owner;
    }
}