using MediatR;

namespace Shared.Models.Event;

public abstract class BaseEvent : INotification
{
    public DateTime OccuredAt { get; protected set; } = DateTime.UtcNow;
}
