namespace Shared.Models.Event;

public abstract class EntityWithEvents
{
    private readonly List<BaseEvent> _events = new();

    public BaseEvent[] GetEventsToPublish()
    {
        var eventsCopy = new BaseEvent[_events.Count];

        _events.CopyTo(eventsCopy);

        _events.Clear();

        return eventsCopy;
    }

    public void AddEvent(BaseEvent @event)
    {
        _events.Add(@event);
    }
}
