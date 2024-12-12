namespace Application.Interface
{
    public interface IErrorLogService
    {
        Task LogErrorAsync(Exception exception);
        void LogError(Exception exception);
    }
}
