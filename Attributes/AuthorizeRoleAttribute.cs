using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupertronicsRepairSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SupertronicsRepairSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAsyncActionFilter
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthService)) as IAuthService;

            if (authService == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var isAuthenticated = await authService.IsAuthenticatedAsync();

            if (!isAuthenticated)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            var userInfo = await authService.GetCurrentUserInfoAsync();

            if (userInfo == null || !_allowedRoles.Contains(userInfo.Role))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }

    // Convenience attributes for specific roles
    public class AuthorizeCustomerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeCustomerAttribute() : base(UserRole.Customer) { }
    }

    public class AuthorizeTechnicianAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeTechnicianAttribute() : base(UserRole.Technician) { }
    }

    public class AuthorizeOwnerAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeOwnerAttribute() : base(UserRole.Owner) { }
    }

    // Allow multiple roles
    public class AuthorizeStaffAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeStaffAttribute() : base(UserRole.Owner, UserRole.Technician) { }
    }
}