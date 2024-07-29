using MalignEngine;
using Arch.Core;
using Microsoft.Xna.Framework;
using Arch.Core.Extensions;

class GameMain : GraphicsApplication
{
    private AssetRegistry assetRegistry;
    private EventBus eventBus;

    private World world;
    private SystemGroup systemGroup;

    private RenderingSystem renderingSystem;

    public override void Initialize()
    {
        assetRegistry = new AssetRegistry();
        eventBus = new EventBus();
        world = World.Create();

        IoCManager.Register(world);
        IoCManager.Register(assetRegistry);
        IoCManager.Register(eventBus);

        systemGroup = new SystemGroup(new List<BaseSystem>
        {
            new EntityEventSystem(),
            new Transform2DSystem(),
            new Physics2DSystem(),
            new RenderingSystem(),
            new SpriteRenderingSystem()
        });

        renderingSystem = IoCManager.Resolve<RenderingSystem>();

        systemGroup.Initialize();

        Texture2D texture = Texture2D.CreateFromFile("Content/Textures/player.png");
        Sprite sprite = new Sprite("player", texture);
        assetRegistry.Add(sprite);

        Entity entity = world.Create(new Position2D(200, 200), new SpriteRenderer() { Sprite = assetRegistry.Get<Sprite>("player"), Color = Color.White });
        entity.Add(new PhysicsBody2D() { BodyType = PhysicsBodyType.Dynamic });
        entity.Add(new BoxCollider2D() { Size = new Vector2(32, 32), Density = 1 });
    }

    public override void Update(float deltaTime)
    {
        systemGroup.Update(deltaTime);
    }

    public override void Draw(float deltaTime)
    {
        renderingSystem.ClearColor(Color.CornflowerBlue);
        systemGroup.Draw(deltaTime);
    }
}