using Application.Accounts.Commands;
using Common.Configurations;
using Common.Exceptions;
using Common.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Controllers
{
    public class AccountsController : BaseController
    {
        public AccountsController(IOptions<ApplicationConfiguration> options) : base(options) { }

        [AllowAnonymous]
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

        [Authorize(Policy = "Mfa Endpoint")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("resend-mfa-token")]
        public async Task<IActionResult> ResendMfaToken([FromBody] ResendMfaTokenCommand command, CancellationToken cancellationToken)
        {
            try
            {
                command.ClientUrl = ClientUrl;
                command.CurrentUserId = CurrentUserId;
                var response = await Mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] AccountSignupCommand command)
        {
            try
            {
                command.ClientUrl = ClientUrl;
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

        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword([FromBody] RequestResetAccountPasswordCommand command)
        {
            try
            {
                command.ClientUrl = ClientUrl;
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

        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Unit), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetAccountPasswordCommand command)
        {
            try
            {
                command.ClientUrl = ClientUrl;
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

        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateAccountCommand command)
        {
            try
            {
                command.ClientUrl = ClientUrl;
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


        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(404)]
        [HttpPost("enable-mfa")]
        public async Task<IActionResult> EnableMfa([FromBody] EnableMfaCommand command, CancellationToken cancellationToken)
        {
            try
            {
                command.ClientUrl = ClientUrl;
                command.CurrentUserId = CurrentUserId;
                var response = await Mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(404)]
        [HttpPost("disable-mfa")]
        public async Task<IActionResult> DisableMfa([FromBody] DisableMfaCommand command, CancellationToken cancellationToken)
        {
            try
            {
                command.ClientUrl = ClientUrl;
                command.CurrentUserId = CurrentUserId;
                var response = await Mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
