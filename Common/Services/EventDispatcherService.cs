using Common.Interfaces;
using MediatR;

namespace Common.Services
{
    public class EventDispatcherService : IEventDispatcherService
    {
        private readonly IMediator _mediator;
        private readonly List<INotification> _notificationQueues = new List<INotification>();
        public EventDispatcherService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void ClearQueue()
        {
            _notificationQueues.Clear();
        }

        public async Task Dispatch(CancellationToken cancellationToken)
        {
            while (_notificationQueues.Count > 0)
            {
                try
                {
                    await _mediator.Publish(_notificationQueues[0], cancellationToken);
                }
                catch (Exception e)
                {
                    // todo : let each notification define what to do on failure
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    _notificationQueues.RemoveAt(0);
                }
            }
        }

        public void QueueNotification(INotification notification)
        {
            _notificationQueues.Add(notification);
        }
    }
}
