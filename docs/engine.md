# Engine Design

# Introduction

The engine is the core of the game. It is responsible for managing the game state, updating the game state, and rendering the game state. The engine is designed to be modular and extensible, allowing for easy modification and customization.

# Core Parts

The engine is composed of several core parts:

## Application

The application is the entry point of the engine. It holds the service container and implements a basic console log method. The application doesn't do much by itself, but it's what will start the first services.

## Services

Everything in the engine is a service. Services are responsible for managing a specific aspect of the game state. For example, the input service is responsible for handling user input, while the physics service is responsible for handling physics calculations.

Services might depend on other services, and they can reference each other through Dependency Injection.

### Logger Service

The logger service is responsible for logging messages to the console. It is used by other services to log messages, warnings, and errors.

It uses a sawmill pattern, where each log message is associated with a severity level (info, warning, error) and a source (the service that generated the message). The logger services doesn't handle the output, so something else must be implemented to handle the output.

Each service can create a sawmill with a custom name, and the logger service will handle the output of all sawmills.

Sawmills can also have parents, so they can be nested.

```cs
```cs
public void class MyService : IService, IUpdate
{
    [Dependency]
    protected ILoggerService LoggerService { get; set; }

    private ISawmill _sawmill;

    public void OnInitialize()
    {
        _sawmill = LoggerService.GetSawmill("myservice");
    }

    public void OnUpdate()
    {
        _sawmill.LogVerbose("This is a verbose message");
    }
}
```

### Schedule Manager

The Schedule Manager is responsible for handling global calls for services, its the most important service in the engine, handling for example the `OnUpdate` and `OnDraw` calls. It's also very fast and with basically no overhead.

Schedules are dispatched by the Schedule Manager and can be listened to by other services. Schedules are implemented through interfaces.

```cs
public void class MyService : IService, IUpdate
{
    public void OnUpdate()
    {
        // Do something
    }
}
```

The Schedule Manager implements the `SubscribeAll` method, which subscribes the service to all events it implements. It also implements the `Run<T>` method, which runs a schedule.

### Event Service


### Entity Manager Service

This is the service responsible for managing entities. It handles the creation, deletion, and updating of entities. It also handles the creation and deletion of components. It also handles the lifecycle of entities and components. Most of it is a wrapper around Arch.

### Asset Service

The asset service is responsible for loading and keeping track of assets. It can load assets from files or from memory. It can also unload assets when they are no longer needed.
It can load assets of different types, such as textures, sounds, and models. It can also load assets of different formats, such as PNG, WAV, and FBX.
It can also cache assets, so they can be reused without having to reload them from disk or memory.

The asset service uses AssetPath, which looks like this: `Content/SomeFile.xml#Identifier` which is the path to the file and the identifier of the asset inside the file. For XML files the identifier is specified in each element, we could also have for example a model file with multiple assets, in that case the identifier would be the name of the asset.

For that we have the `AssetLoader` class, which is responsible for loading assets from files. AssetLoaders get created by AssetLoaderFactory, which is responsible for creating the correct loaders for the file, since we can have multiple assets in a single file.

### Scenes

List of entities in a separate world.

When instantiating a scene, it will copy each entity and component to the main world.

Scenes can be serialized/deseialized to XML. They can also be manually created with code.


### Rendering

TODO: Abstract the rendering into 2 parts. 
The rendering API: Very low level, just sends render commands to the GPU.
The rendering service: High level, uses the rendering API, handles quads, sprites, and other high level rendering tasks.