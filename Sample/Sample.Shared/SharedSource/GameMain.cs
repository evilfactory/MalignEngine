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

    Sprite player;
    Sprite white;
    Sprite fuck;

    public GameMain()
    {
    }

    public override void OnInitialize()
    {
        inputSystem = IoCManager.Resolve<InputSystem>();
        renderingSystem = IoCManager.Resolve<RenderingSystem>();
        assetSystem = IoCManager.Resolve<AssetSystem>();

        player = new Sprite(assetSystem.Load<Texture2D>("Content/Textures/player.png"));
        white = new Sprite(Texture2D.White);
        fuck = new Sprite(assetSystem.Load<Texture2D>("Content/Textures/luatrauma.png"));

        var worldSystem = IoCManager.Resolve<WorldSystem>();

        camera = worldSystem.World.Create(new GlobalLight2D() { Color = Color.White }, new Transform(new Vector2(0, 0)), new OrthographicCamera() { ViewSize = 3f, IsMain = true, PostProcessingSteps = new PostProcessBaseSystem[] { IoCManager.Resolve<LightingPostProcessingSystem2D>() } });

        Entity entity = worldSystem.World.Create(new Transform(new Vector2(0, 5)), new Depth(0f), new SpriteRenderer() { Sprite = player, Color = Color.White });
        entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });

        Entity light = worldSystem.World.Create(
            new Transform(new Vector2(0, 0), 0f, new Vector2(3f, 3f)), 
            new Light2D() { Texture = assetSystem.Load<Texture2D>("Content/Textures/lightcone.png"), Color = Color.White },
            new ParentOf() { Parent = entity.Reference() });

        Entity entity2 = worldSystem.World.Create(new Transform(new Vector2(0, 0), 0f, new Vector2(10000, 1)), new SpriteRenderer() { Sprite = white, Color = Color.White });
        entity2.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static, Mass = 50, Fixtures = new FixtureData2D[] 
        {
            new FixtureData2D(new RectangleShape2D(10000, 1), 1, 1, 1)
        } });

        Entity entity3 = worldSystem.World.Create(new Transform(new Vector2(0, -2f)), new Light2D { Texture = assetSystem.Load<Texture2D>("Content/Textures/lightcone.png") });
    }

    public override void OnUpdate(float deltaTime)
    {
        float speed = 20f;

        if (inputSystem.IsKeyDown(Key.W))
        {
            camera.Get<Transform>().Position += new Vector3(0, 1, 0) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.S))
        {
            camera.Get<Transform>().Position += new Vector3(0, -1, 0) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.A))
        {
            camera.Get<Transform>().Position += new Vector3(-1, 0, 0) * deltaTime * speed;
        }
        if (inputSystem.IsKeyDown(Key.D))
        {
            camera.Get<Transform>().Position += new Vector3(1, 0, 0) * deltaTime * speed;
        }

        if (inputSystem.IsKeyDown(Key.Space))
        {
            var worldSystem = IoCManager.Resolve<WorldSystem>();
            var cameraSystem = IoCManager.Resolve<CameraSystem>();

            Vector2 position = cameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

            Entity entity = worldSystem.World.Create(new Transform(position), new Depth(0f), new SpriteRenderer() { Sprite = player, Color = Color.White });
            entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });
        }

        camera.Get<OrthographicCamera>().ViewSize -= inputSystem.MouseScroll * deltaTime * 10f;
    }

    public override void OnDraw(float deltaTime)
    {

    }
}