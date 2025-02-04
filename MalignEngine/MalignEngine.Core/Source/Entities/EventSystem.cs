namespace MalignEngine
{
    public class EventSystem : IService
    {
        private Dictionary<Type, List<object>> eventSubscribers = new();

        public void SubscribeAll(object observer)
        {
            foreach (var type in observer.GetType().GetInterfaces().Where(inter => inter.IsAssignableTo(typeof(IEvent))))
            {
                if (!eventSubscribers.ContainsKey(type))
                {
                    eventSubscribers.Add(type, new List<object>());
                }

                if (eventSubscribers[type].Contains(observer)) { return; }

                eventSubscribers[type].Add(observer);
            }
        }

        public void PublishEvent<T>(Action<T> eventInvoker) where T : IEvent
        {
            if (!eventSubscribers.ContainsKey(typeof(T))) { return; }

            foreach (var subscriber in eventSubscribers[typeof(T)])
            {
                eventInvoker((T)subscriber);
            }

            return;
        }
    }
}