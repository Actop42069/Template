using Application.Interface;
using Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Application.ErrorLogs.Query
{
    public class ListErrorQuery : IRequest<List<ListErrorResponse>>
    {
        [JsonIgnore]
        public string Id { get; set; }
    }

    public class ListErrorResponse
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string ErrorType { get; set; }
    }

    public class ListErrorHandler : IRequestHandler<ListErrorQuery,List<ListErrorResponse>>
    {
        private readonly ITemplateDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public ListErrorHandler(ITemplateDbContext templateDbContext,
                                IIdentityService identityService)
        {
            _dbContext = templateDbContext;
            _identityService = identityService;
        }

        public async Task<List<ListErrorResponse>> Handle(ListErrorQuery query, CancellationToken cancellationToken)
        {
            var dbUser = await _identityService.GetByIdAsync(query.Id);
            var validRoles = new string[] { "Super Admin", "Admin" };

            if (!dbUser.UserRoles.Select(s => s.Role.Name).Any(a => validRoles.Contains(a)))
                throw new BadRequestException("User does not have a valid role");

            var errorLogs = await _dbContext.ErrorLog
                                              .OrderByDescending(log => log.Timestamp) 
                                              .Select(log => new ListErrorResponse
                                              {
                                                  Id = log.Id,
                                                  Timestamp = log.Timestamp,
                                                  Message = log.Message,
                                                  StackTrace = log.StackTrace,
                                                  ErrorType = log.ErrorType
                                              })
                                              .ToListAsync(cancellationToken);
            return errorLogs;
        }
    }
}
