using Common.Interfaces;
using MediatR;

namespace Domain.Events
{
    public class CreatedEvent : INotification
    {
        public CreatedEvent(ICreatedEvent entity)
        {
            Entity = entity;
        }

        private object Entity { get; }

        public T GetEntity<T>() where T : class
        {
            if (Entity is T entity)
            {
                return entity;
            }
            return null;
        }
    }
}
