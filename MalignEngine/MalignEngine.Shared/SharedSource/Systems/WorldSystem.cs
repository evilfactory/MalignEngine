using Arch.Core;
using Arch.Core.Extensions;
using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{

    public sealed class WorldSystem : BaseSystem
    {
        public World World
        {
            get => world;
        }

        private World world = default!;

        public WorldSystem()
        {
            world = World.Create();
            IoCManager.Register(world);
        }

    }

}