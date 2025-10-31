using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using SupertronicsRepairSystem.Models;
using SupertronicsRepairSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Services
{
    // Firebase authentication service implementation
    public class FirebaseAuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FirestoreDb _firestoreDb;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly FirebaseAuth _firebaseAuth;
        private const string AuthCookieName = "FirebaseAuth";
        private const string UserRoleCookieName = "UserRole";

        public FirebaseAuthService(
            IHttpContextAccessor httpContextAccessor,
            FirestoreDb firestoreDb,
            string firebaseApiKey,
            IHttpClientFactory httpClientFactory,
            FirebaseAuth firebaseAuth)
        {
            _httpContextAccessor = httpContextAccessor;
            _firestoreDb = firestoreDb;
            _apiKey = firebaseApiKey ?? throw new ArgumentNullException(nameof(firebaseApiKey));
            _httpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
            _firebaseAuth = firebaseAuth;
        }

        // Sign in user with Firebase Auth
        public async Task<AuthResult> SignInAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var http = _httpContextAccessor.HttpContext;
                if (http == null)
                {
                    return new AuthResult { Success = false, Message = "No HTTP context available" };
                }

                // Call Firebase Auth API
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
                var payload = new { email, password, returnSecureToken = true };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(requestUri, content);
                var respContent = await resp.Content.ReadAsStringAsync();

                // Handles API errors
                if (!resp.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(respContent);
                        if (doc.RootElement.TryGetProperty("error", out var err) &&
                            err.TryGetProperty("message", out var msg))
                        {
                            return new AuthResult { Success = false, Message = msg.GetString() ?? "Invalid credentials" };
                        }
                    }
                    catch { }

                    return new AuthResult { Success = false, Message = "Invalid credentials" };
                }

                // Parse response tokens
                using var json = JsonDocument.Parse(respContent);
                var root = json.RootElement;
                var idToken = root.GetProperty("idToken").GetString();
                var localId = root.GetProperty("localId").GetString();
                var userEmail = root.GetProperty("email").GetString();

                if (string.IsNullOrEmpty(localId))
                {
                    return new AuthResult { Success = false, Message = "Invalid credentials" };
                }

                // Get user role from Firestore
                var role = await GetUserRoleFromFirestore(localId);
                if (role == null)
                {
                    return new AuthResult { Success = false, Message = "User account not properly configured" };
                }

                // Set auth cookies (need this to make sure it works properly)
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24)
                };

                http.Response.Cookies.Append(AuthCookieName, idToken ?? string.Empty, cookieOptions);
                http.Response.Cookies.Append(UserRoleCookieName, role.Value.ToString(), cookieOptions);

                return new AuthResult
                {
                    Success = true,
                    Message = "Sign in successful",
                    UserId = localId,
                    Email = userEmail,
                    Role = role.Value,
                    IdToken = idToken
                };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "An error occurred during sign in" };
            }
        }

        // Register new user
        public async Task<AuthResult> SignUpAsync(string email, string password, string firstName, string surname, string phoneNumber, UserRole role)
        {
            try
            {
                // Call Firebase Auth signup API
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
                var payload = new { email, password, returnSecureToken = true };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(requestUri, content);
                var respContent = await resp.Content.ReadAsStringAsync();

                // Handle API errors
                if (!resp.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(respContent);
                        if (doc.RootElement.TryGetProperty("error", out var err) &&
                            err.TryGetProperty("message", out var msg))
                        {
                            return new AuthResult { Success = false, Message = msg.GetString() ?? "Failed to create account" };
                        }
                    }
                    catch { }

                    return new AuthResult { Success = false, Message = "Failed to create account" };
                }

                // Parse response
                using var json = JsonDocument.Parse(respContent);
                var root = json.RootElement;
                var idToken = root.GetProperty("idToken").GetString();
                var localId = root.GetProperty("localId").GetString();
                var userEmail = root.GetProperty("email").GetString();

                if (string.IsNullOrEmpty(localId))
                {
                    return new AuthResult { Success = false, Message = "Failed to create account" };
                }

                // Creates user document in Firestore
                await CreateUserDocumentInFirestore(localId, userEmail ?? email, firstName, surname, phoneNumber, role);

                return new AuthResult
                {
                    Success = true,
                    Message = "Account created successfully",
                    UserId = localId,
                    Email = userEmail,
                    Role = role,
                    IdToken = idToken
                };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "An error occurred during sign up" };
            }
        }

        // Signs out and clear cookies
        public async Task SignOutAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return;

            var cookieOptions = new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };
            http.Response.Cookies.Delete(AuthCookieName);
            http.Response.Cookies.Delete(UserRoleCookieName);
            await Task.CompletedTask;
        }

        // Get current user ID from token
        public async Task<string> GetCurrentUserIdAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return string.Empty;

            var token = http.Request.Cookies[AuthCookieName];
            if (string.IsNullOrEmpty(token)) return string.Empty;

            try
            {
                var lookup = await LookupUserByIdTokenAsync(token);
                return lookup?.LocalId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Get current user full info
        public async Task<UserInfo> GetCurrentUserInfoAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return null!;

            var token = http.Request.Cookies[AuthCookieName];
            var roleString = http.Request.Cookies[UserRoleCookieName];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(roleString)) return null!;
            if (!Enum.TryParse<UserRole>(roleString, out var role)) return null!;

            try
            {
                var lookup = await LookupUserByIdTokenAsync(token);
                if (lookup == null) return null!;

                var (localId, email, displayName) = lookup.Value;

                // Get user details from Firestore
                var userDoc = await GetUserDocumentFromFirestore(localId, role);

                return new UserInfo
                {
                    UserId = localId,
                    Email = email,
                    Role = role,
                    DisplayName = string.IsNullOrEmpty(displayName) ? email : displayName,
                    FirstName = userDoc?.FirstName ?? string.Empty,
                    Surname = userDoc?.Surname ?? string.Empty,
                    PhoneNumber = userDoc?.PhoneNumber ?? string.Empty
                };
            }
            catch
            {
                return null!;
            }
        }

        // Checks if user is authenticated
        public async Task<bool> IsAuthenticatedAsync()
        {
            var id = await GetCurrentUserIdAsync();
            return !string.IsNullOrEmpty(id);
        }

        // Get ID token from cookie
        public async Task<string> GetIdTokenAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return string.Empty;

            var token = http.Request.Cookies[AuthCookieName];
            return token ?? string.Empty;
        }

        // Sends password reset email
        public async Task<AuthResult> ResetPasswordAsync(string email)
        {
            try
            {
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
                var payload = new { requestType = "PASSWORD_RESET", email };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(requestUri, content);
                var respContent = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(respContent);
                        if (doc.RootElement.TryGetProperty("error", out var err) &&
                            err.TryGetProperty("message", out var msg))
                        {
                            return new AuthResult { Success = false, Message = msg.GetString() ?? "Failed to send reset email" };
                        }
                    }
                    catch { }

                    return new AuthResult { Success = false, Message = "Failed to send reset email" };
                }

                return new AuthResult { Success = true, Message = "Password reset email sent successfully" };
            }
            catch
            {
                return new AuthResult { Success = false, Message = "An error occurred while sending reset email" };
            }
        }

        // Gets user role from Firestore collections
        private async Task<UserRole?> GetUserRoleFromFirestore(string userId)
        {
            try
            {
                var customerDoc = await _firestoreDb.Collection("customers").Document(userId).GetSnapshotAsync();
                if (customerDoc.Exists) return UserRole.Customer;

                var technicianDoc = await _firestoreDb.Collection("technicians").Document(userId).GetSnapshotAsync();
                if (technicianDoc.Exists) return UserRole.Technician;

                var ownerDoc = await _firestoreDb.Collection("owners").Document(userId).GetSnapshotAsync();
                if (ownerDoc.Exists) return UserRole.Owner;

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Get user document from Firestore by role
        private async Task<(string FirstName, string Surname, string PhoneNumber)?> GetUserDocumentFromFirestore(string userId, UserRole role)
        {
            try
            {
                // Determine collection based on role
                string collection = role switch
                {
                    UserRole.Technician => "technicians",
                    UserRole.Owner => "owners",
                    UserRole.Customer => "customers",
                    _ => "customers"
                };

                var docSnapshot = await _firestoreDb.Collection(collection).Document(userId).GetSnapshotAsync();

                if (!docSnapshot.Exists) return null;

                var firstName = docSnapshot.ContainsField("firstName") ? docSnapshot.GetValue<string>("firstName") : string.Empty;
                var surname = docSnapshot.ContainsField("surname") ? docSnapshot.GetValue<string>("surname") : string.Empty;
                var phoneNumber = docSnapshot.ContainsField("phoneNumber") ? docSnapshot.GetValue<string>("phoneNumber") : string.Empty;

                return (firstName, surname, phoneNumber);
            }
            catch
            {
                return null;
            }
        }

        // Creates user document in Firestore
        private async Task CreateUserDocumentInFirestore(string userId, string email, string firstName, string surname, string phoneNumber, UserRole role)
        {
            var userData = new Dictionary<string, object>
            {
                { "email", email },
                { "firstName", firstName },
                { "surname", surname },
                { "phoneNumber", phoneNumber },
                { "createdAt", FieldValue.ServerTimestamp }
            };

            // Determine collection based on role
            string collection = role switch
            {
                UserRole.Technician => "technicians",
                UserRole.Owner => "owners",
                UserRole.Customer => "customers",
                _ => "customers"
            };

            await _firestoreDb.Collection(collection).Document(userId).SetAsync(userData);
        }

        // Lookup user by ID token via Firebase API
        private async Task<(string LocalId, string Email, string DisplayName)?> LookupUserByIdTokenAsync(string idToken)
        {
            try
            {
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";
                var payload = new { idToken };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _httpClient.PostAsync(requestUri, content);
                var respContent = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode) return null;

                using var json = JsonDocument.Parse(respContent);
                var root = json.RootElement;

                if (!root.TryGetProperty("users", out var users) || users.GetArrayLength() == 0) return null;

                var user = users[0];
                var localId = user.TryGetProperty("localId", out var lid) ? lid.GetString() : null;
                var email = user.TryGetProperty("email", out var em) ? em.GetString() : null;
                var displayName = user.TryGetProperty("displayName", out var dn) ? dn.GetString() : null;

                if (string.IsNullOrEmpty(localId)) return null;
                return (localId, email ?? string.Empty, displayName ?? string.Empty);
            }
            catch
            {
                return null;
            }
        }

        // Get all technicians from DB
        public async Task<List<UserInfo>> GetAllTechniciansAsync()
        {
            var users = new List<UserInfo>();
            try
            {
                var collectionRef = _firestoreDb.Collection("technicians");
                var snapshot = await collectionRef.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    var userInfo = new UserInfo
                    {
                        UserId = document.Id,
                        Role = UserRole.Technician,
                        FirstName = document.ContainsField("firstName") ? document.GetValue<string>("firstName") : string.Empty,
                        Surname = document.ContainsField("surname") ? document.GetValue<string>("surname") : string.Empty,
                        Email = document.ContainsField("email") ? document.GetValue<string>("email") : string.Empty,
                        PhoneNumber = document.ContainsField("phoneNumber") ? document.GetValue<string>("phoneNumber") : string.Empty,
                    };

                    users.Add(userInfo);
                }
            }
            catch
            {
                Console.WriteLine("Error retrieving technicians.");
            }
            return users;
        }

        // Get technician by ID from DB
        public async Task<UserInfo> GetTechnicianByIdAsync(string userId)
        {
            try
            {
                var docRefrence = _firestoreDb.Collection("technicians").Document(userId);
                var snapshot = await docRefrence.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    var userInfo = new UserInfo
                    {
                        UserId = snapshot.Id,
                        Role = UserRole.Technician,
                        FirstName = snapshot.ContainsField("firstName") ? snapshot.GetValue<string>("firstName") : string.Empty,
                        Surname = snapshot.ContainsField("surname") ? snapshot.GetValue<string>("surname") : string.Empty,
                        Email = snapshot.ContainsField("email") ? snapshot.GetValue<string>("email") : string.Empty,
                        PhoneNumber = snapshot.ContainsField("phoneNumber") ? snapshot.GetValue<string>("phoneNumber") : string.Empty,
                    };
                    return userInfo;
                }
                return null!;
            }
            catch
            {
                Console.WriteLine($"Error getting technician by id");
                return null;
            }
        }

        // Updates technician in DB and Firebase Auth
        public async Task<bool> UpdateTechnicianAsync(string userId, EditTechnicianViewModel model)
        {
            bool firestoreSuccess = false;
            try
            {
                // Updates Firestore document
                var updates = new Dictionary<string, object>
                {
                    { "firstName", model.FirstName },
                    { "surname", model.Surname },
                    { "email", model.Email },
                    { "phoneNumber", model.PhoneNumber }
                };

                var techRef = _firestoreDb.Collection("technicians").Document(userId);
                await techRef.UpdateAsync(updates);
                firestoreSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR (Firestore) ***: Failed to update Firestore for {userId}. Message: {ex.Message}");
                return false;
            }

            try
            {
                // Update Firebase Auth user
                var updateParameters = new UserRecordArgs()
                {
                    Uid = userId,
                    Email = model.Email,
                    DisplayName = $"{model.FirstName} {model.Surname}"
                };

                await _firebaseAuth.UpdateUserAsync(updateParameters);
                Console.WriteLine($"SUCCESS: Updated user {userId} in Firebase Authentication.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Failed to update technician in Firebase Authentication {userId}. Error: {ex.Message}");
                return false;
            }
            return true;
        }

        // Delete technician from DB and Firebase Auth
        public async Task<bool> DeleteTechnicianAsync(string userId)
        {
            bool firestoreDeleted = false;

            try
            {
                // Delete from Firestore
                var techRef = _firestoreDb.Collection("technicians").Document(userId);
                await techRef.DeleteAsync();
                firestoreDeleted = true;
            }
            catch
            {
                Console.WriteLine($"Error deleting technician with id {userId}");
                return false;
            }

            try
            {
                // Delete from Firebase Auth
                await _firebaseAuth.DeleteUserAsync(userId);
                Console.WriteLine($"SUCCESS: Deleted user {userId} from Firebase Authentication.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Failed to delete technician from Firebase Authentication {userId}. Error: {ex.Message}");
            }

            return firestoreDeleted;
        }
    }
}