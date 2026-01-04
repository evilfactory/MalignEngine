using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{
    public abstract class EntitySystem : BaseSystem
    {
        protected readonly IEntityManager EntityManager;
        protected readonly IEventService EventService;
        protected IWorld World => EntityManager.World;

        protected EntitySystem(ILoggerService loggerService, IScheduleManager scheduleManager, IEntityManager entityManager, IEventService eventService) 
            : base(loggerService, scheduleManager)
        {
            EntityManager = entityManager;
            EventService = eventService;
        }
    }
}