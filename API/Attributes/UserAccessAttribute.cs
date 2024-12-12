using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Api.Attributes
{
    public class CustomerAccessAttribute : TypeFilterAttribute
    {
        public CustomerAccessAttribute() : base(typeof(CustomerAccessFilter)) { }
    }

    public class CustomerAccessFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var role = context.HttpContext.User.Claims.FirstOrDefault(fd => fd.Type == ClaimTypes.Role).Value;
                if (role != "User")
                {
                    context.Result = new ForbidResult();
                    return;
                }

                var mfaClaim = context.HttpContext.User.Claims.FirstOrDefault(fd => fd.Type == ClaimTypes.AuthenticationMethod)?.Value;
                if (!string.IsNullOrEmpty(mfaClaim))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                return;
            }
        }
    }
}
