using MalignEngine;
using Arch.Core;
using System.Numerics;
using Arch.Core.Extensions;

class GameMain : BaseSystem, IDrawGUI
{
    private SystemGroup systemGroup;

    [Dependency]
    protected AssetSystem AssetSystem = default!;
    [Dependency]
    protected WindowSystem WindowSystem = default!;
    [Dependency]
    protected FontSystem FontSystem = default!;
    [Dependency]
    protected RenderingSystem RenderingSystem = default!;
    [Dependency]
    protected CameraSystem CameraSystem = default!;

    private InputSystem inputSystem;

    private Entity playerEnt;
    private Entity camera;

    Sprite player;
    Sprite white;
    Sprite fuck;

    GUIFrame frame;

    public GameMain()
    {
    }

    public override void OnInitialize()
    {
        GUIStyle.Default = new GUIStyle()
        {
            FrameTexture = AssetSystem.Load<Texture2D>("Content/Textures/luatrauma.png"),
            NormalFont = AssetSystem.Load<Font>("Content/Roboto-Regular.ttf")
        };

        frame = new GUIFrame(new RectTransform(null, new Vector2(1, 1), new Vector2(), Anchor.Center, Pivot.Center), new Color(1f, 0f, 0f, 0.25f));
        frame.RectTransform.AbsoluteSize = new Vector2(WindowSystem.Width, WindowSystem.Height);

        var blueFrame = new GUIFrame(new RectTransform(frame.RectTransform, new Vector2(0.25f, 0.5f), Vector2.Zero, Anchor.TopCenter, Pivot.TopCenter), Color.Blue);

        var blueGuiList = new GUIList(new RectTransform(blueFrame.RectTransform, Vector2.One, Vector2.Zero, Anchor.TopCenter, Pivot.TopCenter), 10f);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Vector2.Zero, Anchor.TopCenter, Pivot.Center), Color.Pink);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Vector2.Zero, Anchor.TopCenter, Pivot.Center), Color.Salmon);
        new GUIText(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.5f), Vector2.Zero, Anchor.TopCenter, Pivot.Center), "Funny", Color.SeaGreen);

        new GUIFrame(new RectTransform(frame.RectTransform, new Vector2(1f, 0.1f), Vector2.Zero, Anchor.BottomCenter, Pivot.Center), Color.Blue);

        inputSystem = IoCManager.Resolve<InputSystem>();

        player = new Sprite(AssetSystem.Load<Texture2D>("Content/Textures/player.png"));
        white = new Sprite(Texture2D.White);
        fuck = new Sprite(AssetSystem.Load<Texture2D>("Content/Textures/luatrauma.png"));

        var worldSystem = IoCManager.Resolve<WorldSystem>();

        camera = worldSystem.World.Create(new GlobalLight2D() { Color = Color.White }, new Transform(new Vector2(0, 0)), new OrthographicCamera() { ViewSize = 3f, IsMain = true, PostProcessingSteps = new PostProcessBaseSystem[] { IoCManager.Resolve<LightingPostProcessingSystem2D>() } });

        playerEnt = worldSystem.World.Create(new Transform(new Vector2(0, 5)), new Depth(0f), new SpriteRenderer() { Sprite = player, Color = Color.White });
        playerEnt.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });

        Entity light = worldSystem.World.Create(
            new Transform(new Vector2(0, 0), 0f, new Vector2(3f, 3f)), 
            new Light2D() { Texture = AssetSystem.Load<Texture2D>("Content/Textures/lightcone.png"), Color = Color.White },
            new ParentOf() { Parent = playerEnt.Reference() });

        Entity entity2 = worldSystem.World.Create(new Transform(new Vector2(0, -3f), 0f, new Vector2(10000, 1)), new SpriteRenderer() { Sprite = white, Color = Color.White });
        entity2.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static, Mass = 50, Fixtures = new FixtureData2D[] 
        {
            new FixtureData2D(new RectangleShape2D(10000, 1), 1, 1, 1)
        } });

        Entity entity3 = worldSystem.World.Create(new Transform(new Vector2(0, -2f)), new Light2D { Texture = AssetSystem.Load<Texture2D>("Content/Textures/lightcone.png") });
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

            Vector2 position = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

            Entity entity = worldSystem.World.Create(new Transform(position), new Depth(0f), new SpriteRenderer() { Sprite = player, Color = Color.White });
            entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });
        }

        camera.Get<OrthographicCamera>().ViewSize -= inputSystem.MouseScroll * deltaTime * 10f;
    }

    public override void OnDraw(float deltaTime)
    {
        Vector2 position = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

        RenderingSystem.Begin();
        FontSystem.DrawFont(AssetSystem.Load<Font>("Content/Roboto-Regular.ttf"), 100, "Hello World", position, Color.Red, 0f, default, new Vector2(0.01f, -0.01f));
        RenderingSystem.End();
    }

    public void OnDrawGUI(float deltaTime)
    {
        frame.RectTransform.AbsoluteSize = new Vector2(WindowSystem.Width, WindowSystem.Height);

        Vector2 position = inputSystem.MousePosition;

        RenderingSystem.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));

        FontSystem.DrawFont(AssetSystem.Load<Font>("Content/Roboto-Regular.ttf"), 100, "funny", position, Color.Red, 0f, default, new Vector2(1f, 1f));

        frame.Draw();
        //RenderingSystem.DrawQuad(Texture2D.White,
        //    new VertexPositionColorTexture(new Vector3(50, 50, 0), Color.Blue, new Vector2(1f, 1f)), // top right
        //    new VertexPositionColorTexture(new Vector3(50, 0, 0), Color.Green, new Vector2(1f, 0f)), // bottom right
        //    new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.Pink, new Vector2(0f, 0f)), // bottom left
        //    new VertexPositionColorTexture(new Vector3(0, 50, 0), Color.Red, new Vector2(0f, 1f)) // top left
        //);
        //RenderingSystem.DrawQuad(player.Texture,
        //    new VertexPositionColorTexture(new Vector3(10, 10, 0), Color.Blue, new Vector2(1f, 1f)), // top right
        //    new VertexPositionColorTexture(new Vector3(10, 5, 0), Color.Green, new Vector2(1f, 0f)), // bottom right
        //    new VertexPositionColorTexture(new Vector3(5, 5, 0), Color.Pink, new Vector2(0f, 0f)), // bottom left
        //    new VertexPositionColorTexture(new Vector3(5, 10, 0), Color.Red, new Vector2(0f, 1f)) // top left
        //);
        //FontSystem.DrawFont(assetSystem.Load<Font>("Content/Roboto-Regular.ttf"), 100, "Hello World", new Vector2(600f, 600f), Color.Red, 0f, default, new Vector2(100f, 100f));
        RenderingSystem.End();
    }
}