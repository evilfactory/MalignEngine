using MalignEngine;
using Arch.Core;
using System.Numerics;
using Arch.Core.Extensions;

class GameMain : BaseSystem
{
    private AssetRegistry assetRegistry;

    private SystemGroup systemGroup;

    private InputSystem inputSystem;
    private RenderingSystem renderingSystem;

    private Entity camera;

    public GameMain()
    {
        assetRegistry = new AssetRegistry();

        IoCManager.Register(assetRegistry);

        systemGroup = new SystemGroup();
        var window = new WindowSystem(systemGroup, "Malign Engine", new Vector2(800, 600));
        systemGroup.AddSystem(new WorldSystem());
        systemGroup.AddSystem(new EventSystem());
        systemGroup.AddSystem(window);
        systemGroup.AddSystem(new GLRenderingSystem());
        systemGroup.AddSystem(new InputSystem());
        systemGroup.AddSystem(new ImGuiSystem());
        systemGroup.AddSystem(new CameraSystem());
        systemGroup.AddSystem(new Transform2DSystem());
        systemGroup.AddSystem(new Physics2DSystem());
        systemGroup.AddSystem(new SpriteRenderingSystem());
        systemGroup.AddSystem(new EditorSystem());
        systemGroup.AddSystem(new EditorInspectorSystem());
        systemGroup.AddSystem(this);

        window.Run();
    }

    public override void Initialize()
    {
        inputSystem = IoCManager.Resolve<InputSystem>();
        renderingSystem = IoCManager.Resolve<RenderingSystem>();

        assetRegistry.Add(new Sprite("player", Texture2D.CreateFromFile("Content/Textures/player.png")));
        assetRegistry.Add(new Sprite("fuck", Texture2D.CreateFromFile("Content/Textures/luatrauma.png")));

        var worldSystem = IoCManager.Resolve<WorldSystem>();

        camera = worldSystem.World.Create(new Position2D(0, 0), new OrthographicCamera() { Zoom = 3f });

        Entity entity = worldSystem.World.Create(new Position2D(0, 0), new Depth(0f), new Rotation2D(0f), new SpriteRenderer() { Sprite = assetRegistry.Get<Sprite>("player"), Color = Color.White });
        entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50 });
        entity.Add(new BoxCollider2D() { Size = new Vector2(1, 1), Density = 1 });

        Entity entity2 = worldSystem.World.Create(new Position2D(0, -3), new SpriteRenderer() { Sprite = assetRegistry.Get<Sprite>("player"), Color = Color.White });
        entity2.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static, Mass = 50 });
        entity2.Add(new BoxCollider2D() { Size = new Vector2(100, 1), Density = 1 });
    }

    public override void Update(float deltaTime)
    {
        float speed = 20f;

        if (inputSystem.IsKeyDown(Key.W))
        {
            camera.Get<Position2D>().Position += new Vector2(0, 1) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.S))
        {
            camera.Get<Position2D>().Position += new Vector2(0, -1) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.A))
        {
            camera.Get<Position2D>().Position += new Vector2(-1, 0) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.D))
        {
            camera.Get<Position2D>().Position += new Vector2(1, 0) * deltaTime * speed;
        }

        if (inputSystem.IsKeyDown(Key.Space))
        {
            var worldSystem = IoCManager.Resolve<WorldSystem>();
            var cameraSystem = IoCManager.Resolve<CameraSystem>();
            var inputSystem = IoCManager.Resolve<InputSystem>();

            Vector2 position = cameraSystem.ScreenToWorld(inputSystem.MousePosition);

            Entity entity = worldSystem.World.Create(new Position2D(position), new Depth(0f), new Rotation2D(0f), new SpriteRenderer() { Sprite = assetRegistry.Get<Sprite>("player"), Color = Color.White });
            entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50 });
            entity.Add(new BoxCollider2D() { Size = new Vector2(1, 1), Density = 1 });
        }

        camera.Get<OrthographicCamera>().Zoom -= inputSystem.MouseScroll * deltaTime * 10f;
    }

    public override void Draw(float deltaTime)
    {

    }
}