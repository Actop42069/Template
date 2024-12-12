using Application.Interface;
using Domain.Entities;
using Infrastructure.Persistance;

namespace Infrastructure.Services
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly TemplateDbContext _dbContext;

        public ErrorLogService(TemplateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogErrorAsync(Exception exception)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    ErrorType = exception.GetType().Name
                };

                _dbContext.Set<ErrorLog>().Add(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); //TODO 
            }
        }

        public void LogError(Exception exception)
        {
            var errorLog = new ErrorLog
            {
                Timestamp = DateTimeOffset.UtcNow,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                ErrorType = exception.GetType().Name
            };

            _dbContext.Set<ErrorLog>().Add(errorLog);
            _dbContext.SaveChanges();
        }

    }
}
