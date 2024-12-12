using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Api.Attributes
{
    public class AdminAccessAttribute : TypeFilterAttribute
    {
        public AdminAccessAttribute() : base(typeof(AdminAccessFilter)) { }
    }

    public class AdminAccessFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var role = context.HttpContext.User.Claims.FirstOrDefault(fd => fd.Type == ClaimTypes.Role).Value;
                if (role == "Super Admin" || role == "Admin")
                    return;

                context.Result = new ForbidResult();
            }
        }
    }
}
