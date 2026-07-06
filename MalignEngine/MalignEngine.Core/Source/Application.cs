using System.Globalization;
using System.Reflection;

namespace MalignEngine;

public abstract class Application
{
    public static Application? Main { get; private set; }

    public abstract IServiceContainer ServiceContainer { get; protected set; }

    public Application()
    {
        Main = this;
    }

    /// <summary>
    /// Instantiates all systems.
    /// </summary>
    public abstract void Initialize();
}