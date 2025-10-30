using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupertronicsRepairSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Attributes
{
    // Custom authorization attribute for role-based access control
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAsyncActionFilter
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        // Check user authentication and role before action execution
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get auth service from DI
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthService)) as IAuthService;

            if (authService == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Verify user is logged in
            var isAuthenticated = await authService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            // Check user has required role
            var userInfo = await authService.GetCurrentUserInfoAsync();
            if (userInfo == null || !_allowedRoles.Contains(userInfo.Role))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }

    // Customer-only access
    public class AuthorizeCustomerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeCustomerAttribute() : base(UserRole.Customer) { }
    }

    // Technician-only access
    public class AuthorizeTechnicianAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeTechnicianAttribute() : base(UserRole.Technician) { }
    }

    // Owner-only access
    public class AuthorizeOwnerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeOwnerAttribute() : base(UserRole.Owner) { }
    }

    // Staff access (Owner and Technician)
    public class AuthorizeStaffAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeStaffAttribute() : base(UserRole.Owner, UserRole.Technician) { }
    }
}