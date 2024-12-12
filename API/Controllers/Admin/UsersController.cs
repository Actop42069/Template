using Application.AdminArea.Users.Commands;
using Application.Users.Query;
using Common.Configurations;
using Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Controllers.Admin
{
    public class UsersController : BaseAdminController
    {
        public UsersController(IOptions<ApplicationConfiguration> options) : base(options) { }

        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ListUsersResponse>), 200)]
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string role = "Admin")
        {
            var query = new ListUsersQuery { Role = role };
            var response = await Mediator.Send(query);
            return Ok(response);
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateUserCommand command)
        {
            try
            {
                command.CurrentUser = CurrentUserName;
                command.ClientUrl = AdminUrl;
                var response = await Mediator.Send(command);
                return Ok(response);
            }
            catch (Exception ex) when (ex is BadRequestException ||
                                       ex is ValidationException ||
                                       ex is DbUpdateException)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("re-invite")]
        public async Task<IActionResult> Reinvite([FromBody] ReinviteUserCommand command)
        {
            try
            {
                command.CurrentUser = CurrentUserName;
                command.ClientUrl = AdminUrl;
                var response = await Mediator.Send(command);
                return Ok(response);
            }
            catch (Exception ex) when (ex is NotFoundException ||
                                       ex is BadRequestException ||
                                       ex is DbUpdateException)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpDelete("{UserId}")]
        public async Task<IActionResult> Delete([FromRoute] DeleteUserCommand command)
        {
            try
            {
                var response = await Mediator.Send(command);
                return Ok(response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
        }
    }
}
