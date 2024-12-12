using Api.Controllers.Admin;
using Application.ErrorLogs.Query;
using Common.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers.Admin
{
    public class ErrorLogController : BaseAdminController
    {
        public ErrorLogController(IOptions<ApplicationConfiguration> options) : base(options) { }

        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ListErrorResponse>), 200)]
        [HttpGet]
        public async Task<IActionResult> ListErrorLog()
        {
            var query = new ListErrorQuery { Id = CurrentUserId };
            var response = await Mediator.Send(query);
            return Ok(response);
        }
    }
}
