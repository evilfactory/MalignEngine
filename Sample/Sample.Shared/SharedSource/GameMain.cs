using MalignEngine;
using Arch.Core;
using System.Numerics;
using Arch.Core.Extensions;

class GameMain : BaseSystem
{
    private SystemGroup systemGroup;

    private InputSystem inputSystem;
    private RenderingSystem renderingSystem;
    private AssetSystem assetSystem;

    private Entity camera;

    public GameMain()
    {
        systemGroup = new SystemGroup();
        var window = new WindowSystem("Malign Engine", new Vector2(800, 600));
        systemGroup.AddSystem(new WorldSystem());
        systemGroup.AddSystem(new EventSystem());
        systemGroup.AddSystem(new AssetSystem());
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
        systemGroup.AddSystem(new EditorPerformanceSystem());
        systemGroup.AddSystem(new EditorSceneViewSystem());
        systemGroup.AddSystem(this);

        window.Run();
    }

    public override void Initialize()
    {
        inputSystem = IoCManager.Resolve<InputSystem>();
        renderingSystem = IoCManager.Resolve<RenderingSystem>();
        assetSystem = IoCManager.Resolve<AssetSystem>();

        assetSystem.Add(new Sprite("white", Texture2D.White));
        assetSystem.Add(new Sprite("player", Texture2D.CreateFromFile("Content/Textures/player.png")));
        assetSystem.Add(new Sprite("fuck", Texture2D.CreateFromFile("Content/Textures/luatrauma.png")));

        var worldSystem = IoCManager.Resolve<WorldSystem>();

        camera = worldSystem.World.Create(new Position2D(0, 0), new OrthographicCamera() { ViewSize = 3f, IsMain = true });

        Entity entity = worldSystem.World.Create(new Position2D(0, 0), new Depth(0f), new Rotation2D(0f), new SpriteRenderer() { Sprite = assetSystem.Get<Sprite>("player"), Color = Color.White });
        entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50 });
        entity.Add(new BoxCollider2D() { Size = new Vector2(1, 1), Density = 1 });

        Entity entity2 = worldSystem.World.Create(new Position2D(0, -3), new Scale2D(10000, 1), new SpriteRenderer() { Sprite = assetSystem.Get<Sprite>("white"), Color = Color.White });
        entity2.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static, Mass = 50 });
        entity2.Add(new BoxCollider2D() { Size = new Vector2(10000, 1), Density = 1 });
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

            Vector2 position = cameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

            Entity entity = worldSystem.World.Create(new Position2D(position), new Depth(0f), new Rotation2D(0f), new SpriteRenderer() { Sprite = assetSystem.Get<Sprite>("player"), Color = Color.White });
            entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50 });
            entity.Add(new BoxCollider2D() { Size = new Vector2(1, 1), Density = 1 });
        }

        camera.Get<OrthographicCamera>().ViewSize -= inputSystem.MouseScroll * deltaTime * 10f;
    }

    public override void Draw(float deltaTime)
    {

    }
}