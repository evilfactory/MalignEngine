using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{
    public abstract class EntitySystem : BaseSystem
    {
        protected readonly IEntityManager EntityManager = default!;
        //protected readonly IEventService EventService = default!; 

        protected IWorld World => EntityManager.World;

        protected EntitySystem(IServiceContainer serviceContainer) : base(serviceContainer)
        {
            EntityManager = serviceContainer.GetInstance<IEntityManager>();
            //EventService = serviceContainer.GetInstance<IEventService>();
        }
    }
}