using Api.Controllers.Admin;
using Application.AdminArea.Accounts.Commands;
using Common.Configurations;
using Common.Exceptions;
using Common.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Controllers.Admin
{
    [AllowAnonymous]
    public class AccountsController : BaseAdminController
    {
        public AccountsController(IOptions<ApplicationConfiguration> options) : base(options) { }

        [Produces("application/json")]
        [ProducesResponseType(typeof(AccountLoginResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountLoginCommand command)
        {
            try
            {
                var response = await Mediator.Send(command);
                return Ok(response);
            }
            catch (Exception ex) when (ex is BadRequestException ||
                                       ex is NotFoundException)
            {
                return BadRequest(ex.InnerException?.Message ?? ExceptionMessage.InvalidEmailPassword);
            }
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword([FromBody] RequestResetAccountPasswordCommand command)
        {
            try
            {
                command.ClientUrl = AdminUrl;
                var response = await Mediator.Send(command);
                return Ok(response);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (ex is NotFoundException ||
                                       ex is ValidationException)
            {
                return Ok();
            }
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetAccountPasswordCommand command)
        {
            try
            {
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
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] Application.Accounts.Commands.ActivateAccountCommand command)
        {
            try
            {
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
    }
}
