using MalignEngine;
using Arch.Core;
using System.Numerics;
using Arch.Core.Extensions;

class GameMain : BaseSystem, IDrawGUI
{
    [Dependency]
    protected AssetService AssetService = default!;
    [Dependency]
    protected WindowSystem WindowSystem = default!;
    [Dependency]
    protected FontSystem FontSystem = default!;
    [Dependency]
    protected RenderingSystem RenderingSystem = default!;
    [Dependency]
    protected CameraSystem CameraSystem = default!;
    [Dependency]
    protected EntityManagerService EntityManager = default!;

    private InputSystem inputSystem;

    private EntityRef playerEnt;
    private EntityRef camera;

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
            FrameTexture = Texture2D.White,
            NormalFont = AssetService.FromFile<Font>("Content/Roboto-Regular.ttf")
        };

        frame = new GUIFrame(new RectTransform(null, new Vector2(1, 1), Anchor.Center, Pivot.Center), new Color(1f, 0f, 0f, 0.25f));
        frame.RectTransform.AbsoluteSize = new Vector2(WindowSystem.Width, WindowSystem.Height);

        var blueFrame = new GUIFrame(new RectTransform(frame.RectTransform, new Vector2(0.25f, 0.5f), Anchor.TopCenter, Pivot.TopCenter), Color.Blue);
        blueFrame.RectTransform.AbsoluteOffset = new Vector2(0, 0);
        var blueGuiList = new GUIList(new RectTransform(blueFrame.RectTransform, Vector2.One, Anchor.TopCenter, Pivot.TopCenter), 10f);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), Color.Pink);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), Color.Salmon);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), Color.Salmon);
        new GUIFrame(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), Color.Salmon);
        var button = new GUIButton(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.5f), Anchor.TopCenter, Pivot.TopCenter), () => { });
        button.DefaultColor = Color.Black;
        button.HoverColor = Color.Gray;
        button.ClickColor = Color.BlueViolet;
        new GUIText(new RectTransform(button.RectTransform, new Vector2(0.8f, 0.5f), Anchor.Center, Pivot.Center), "Button", Color.SeaGreen);
        new GUIText(new RectTransform(blueGuiList.RectTransform, new Vector2(0.8f, 0.5f), Anchor.TopCenter, Pivot.TopCenter), "Funny", Color.SeaGreen);

        new GUIFrame(new RectTransform(frame.RectTransform, new Vector2(1f, 0.1f), Anchor.BottomCenter, Pivot.Center), Color.Blue);

        inputSystem = IoCManager.Resolve<InputSystem>();

        player = new Sprite(AssetService.FromFile<Texture2D>("Content/Textures/player.png"));
        white = new Sprite(Texture2D.White);
        fuck = new Sprite(AssetService.FromFile<Texture2D>("Content/Textures/luatrauma.png"));

        camera = EntityManager.World.CreateEntity();
        camera.Add(new GlobalLight2D() { Color = Color.White });
        camera.Add(new Transform());
        camera.Add(new OrthographicCamera() { ViewSize = 3f, IsMain = true, PostProcessingSteps = new PostProcessBaseSystem[] { IoCManager.Resolve<LightingPostProcessingSystem2D>() } });

        playerEnt = EntityManager.World.CreateEntity();
        playerEnt.Add(new Transform(new Vector2(0, 5)));
        playerEnt.Add(new SpriteRenderer() { Sprite = player, Color = Color.White });
        playerEnt.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });

        EntityRef light = EntityManager.World.CreateEntity();
        light.Add(new Transform(new Vector2(0, 0), 0f, new Vector2(3f, 3f)));
        light.Add(new Light2D() { Texture = AssetService.FromFile<Texture2D>("Content/Textures/lightcone.png"), Color = Color.White });
        light.Add(new ParentOf() { Parent = playerEnt });

        EntityRef entity2 = EntityManager.World.CreateEntity();
        entity2.Add(new Transform(new Vector2(0, -3f), 0f, new Vector2(10000, 1)));
        entity2.Add(new SpriteRenderer() { Sprite = white, Color = Color.White });
        entity2.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Static, Mass = 50, Fixtures = new FixtureData2D[] 
        {
            new FixtureData2D(new RectangleShape2D(10000, 1), 1, 1, 1)
        } });

        EntityRef entity3 = EntityManager.World.CreateEntity();
        entity3.Add(new Transform(new Vector2(0, -2f)));
        entity3.Add(new Light2D { Texture = AssetService.FromFile<Texture2D>("Content/Textures/lightcone.png") });
    }

    public override void OnUpdate(float deltaTime)
    {
        frame.Update();

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
            Vector2 position = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

            EntityRef entity = EntityManager.World.CreateEntity();
            entity.Add(new Transform(position));
            entity.Add(new SpriteRenderer() { Sprite = player, Color = Color.White });
            entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic, Mass = 50, Fixtures = new FixtureData2D[] { new FixtureData2D(new CircleShape2D(1f), 1, 0, 0) } });
        }

        camera.Get<OrthographicCamera>().ViewSize -= inputSystem.MouseScroll * deltaTime * 10f;
    }

    public override void OnDraw(float deltaTime)
    {
        Vector2 position = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), inputSystem.MousePosition);

        RenderingSystem.Begin();
        FontSystem.DrawFont(AssetService.FromFile<Font>("Content/Roboto-Regular.ttf"), 100, "Hello World", position, Color.Red, 0f, default, new Vector2(0.01f, -0.01f));
        RenderingSystem.End();
    }

    public void OnDrawGUI(float deltaTime)
    {
        frame.RectTransform.AbsoluteSize = new Vector2(WindowSystem.Width, WindowSystem.Height);

        Vector2 position = inputSystem.MousePosition;

        RenderingSystem.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));

        FontSystem.DrawFont(AssetService.FromFile<Font>("Content/Roboto-Regular.ttf"), 100, "funny", position, Color.Red, 0f, default, new Vector2(1f, 1f));

        frame.Draw();
        RenderingSystem.DrawQuad(Texture2D.White,
            new VertexPositionColorTexture(new Vector3(500, 500, 0), Color.Blue, new Vector2(1f, 1f)), // top right
            new VertexPositionColorTexture(new Vector3(500, 0, 0), Color.Green, new Vector2(1f, 0f)), // bottom right
            new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.Pink, new Vector2(0f, 0f)), // bottom left
            new VertexPositionColorTexture(new Vector3(0, 500, 0), Color.Red, new Vector2(0f, 1f)) // top left
        );
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