using Arch.Core;
using Arch.Core.Extensions;
using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{
    public abstract class EntitySystem : BaseSystem
    {
        protected readonly IEntityManager EntityManager;
        protected readonly IEventService EventService;

        protected EntitySystem(ILoggerService loggerService, IScheduleManager scheduleManager, IEntityManager entityManager, IEventService eventService) 
            : base(loggerService, scheduleManager)
        {
            EntityManager = entityManager;
            EventService = eventService;
        }

        public static bool Resolve<T>(in EntityRef entity, ref T comp) where T : IComponent
        {
            bool success = false;
            comp = entity.TryGetRef<T>(out success);
            return success;
        }
    }
}