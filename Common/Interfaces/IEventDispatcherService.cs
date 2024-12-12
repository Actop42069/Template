using MediatR;

namespace Common.Interfaces
{
    public interface IEventDispatcherService
    {
        void QueueNotification(INotification notification);
        void ClearQueue();
        Task Dispatch(CancellationToken cancellationToken);
    }
}
