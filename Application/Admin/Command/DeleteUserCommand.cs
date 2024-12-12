using Application.Interface;
using MediatR;

namespace Application.AdminArea.Users.Commands
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public DeleteUserHandler(IIdentityService identityService,
                                  IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUser = await _identityService.GetByIdAsync(request.UserId);
                await _identityService.DeleteAsync(dbUser);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class DeleteUserCommand : IRequest<Unit>
    {
        public string UserId { get; set; }
    }
}
