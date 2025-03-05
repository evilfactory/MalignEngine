using System.Numerics;

namespace MalignEngine.Sample;

public class LightingTest : IService, IStateEnter<GameState>, IUpdate
{
    [Dependency]
    protected EntityManager EntityManagerService = default!;
    [Dependency]
    protected LightingPostProcessingSystem2D LightingPostProcessingSystem2D = default!;
    [Dependency]
    protected InputSystem InputSystem = default!;
    [Dependency]
    protected CameraSystem CameraSystem = default!;

    private EntityRef light;
    private EntityRef camera;
    private float time = 0;

    public void OnStateEnter(GameState state)
    {
        if (state != GameState.LightingTest) { return; }

        camera = EntityManagerService.World.CreateEntity();
        camera.Add(new Transform());
        camera.Add(new OrthographicCamera() { ClearColor = Color.SkyBlue, IsMain = true, ViewSize = 10, PostProcessingSteps = new PostProcessBaseSystem[] { LightingPostProcessingSystem2D } });

        EntityRef testEntity = EntityManagerService.World.CreateEntity();
        testEntity.Add(new Transform() { Position = new Vector3(0, 0, 0), Scale = new Vector3(1, 1, 1) });
        testEntity.Add(new SpriteRenderer() { Sprite = new Sprite(Texture2D.White), Color = Color.Red });

        light = EntityManagerService.World.CreateEntity();
        light.Add(new Transform() { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 10, 1) });
        light.Add(new Light2D() { Texture = new Texture2D().LoadFromPath("Content/Textures/light.png"), Color = Color.White });

        EntityRef staticLight1 = EntityManagerService.World.CreateEntity();
        staticLight1.Add(new Transform() { Position = new Vector3(5, 0, 0), Scale = new Vector3(5, 5, 1) });
        staticLight1.Add(new Light2D() { Texture = new Texture2D().LoadFromPath("Content/Textures/light.png"), Color = Color.White });

        EntityRef staticLight2 = EntityManagerService.World.CreateEntity();
        staticLight2.Add(new Transform() { Position = new Vector3(-5, 0, 0), Scale = new Vector3(5, 5, 1) });
        staticLight2.Add(new Light2D() { Texture = Texture2D.White, Color = Color.White });

    }

    public void OnUpdate(float deltaTime)
    {
        time += deltaTime;

        Vector2 mousePosition = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), InputSystem.MousePosition);

        light.Get<Transform>().Position = mousePosition.ToVector3();
    }
}
