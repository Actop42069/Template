using Common.Configurations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly ApplicationConfiguration _applicationConfiguration;
        public BaseController(IOptions<ApplicationConfiguration> options = default)
        {
            _applicationConfiguration = options?.Value;
        }

        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        protected string CurrentUserId => HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected string CurrentUserName => HttpContext.User.FindFirstValue(ClaimTypes.Name);
        protected string MfaProvider => HttpContext.User?.Claims.FirstOrDefault(f => f.Type == ClaimTypes.AuthorizationDecision)?.Value;

        protected string ClientUrl
        {
            get { return _applicationConfiguration.ClientUrl; }
            set { value = _applicationConfiguration.ClientUrl; }
        }
    }
}
