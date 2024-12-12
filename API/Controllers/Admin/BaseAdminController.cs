using Api.Attributes;
using Common.Configurations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Api.Controllers.Admin
{
    [AdminAccess]
    [Authorize]
    [Route("api/admin/[controller]")]
    [ApiController]
    public abstract class BaseAdminController : ControllerBase
    {
        private readonly ApplicationConfiguration _applicationConfiguration;
        public BaseAdminController(IOptions<ApplicationConfiguration> options = default)
        {
            _applicationConfiguration = options?.Value;
        }

        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        protected string CurrentUserId => HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected string CurrentUserName => HttpContext.User.FindFirstValue(ClaimTypes.Name);

        protected string ClientUrl
        {
            get { return _applicationConfiguration.ClientUrl; }
        }

        protected string AdminUrl
        {
            get { return _applicationConfiguration.AdminUrl; }
        }
    }
}
