using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(string email, string password, bool rememberMe);
        Task<AuthResult> SignUpAsync(string email, string password, UserRole role);
        Task SignOutAsync();
        Task<string> GetCurrentUserIdAsync();
        Task<UserInfo> GetCurrentUserInfoAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetIdTokenAsync();
        Task<AuthResult> ResetPasswordAsync(string email);
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

        public bool IsCustomer => Role == UserRole.Customer;
        public bool IsTechnician => Role == UserRole.Technician;
        public bool IsOwner => Role == UserRole.Owner;
    }
}