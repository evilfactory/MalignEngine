using System.Numerics;

namespace MalignEngine.Sample;

public class CameraTest : IService, IStateEnter<GameState>, IUpdate
{
    [Dependency]
    protected EntityManagerService EntityManagerService = default!;

    private EntityRef camera;
    private float time = 0;

    public void OnStateEnter(GameState state)
    {
        if (state != GameState.CameraTest) { return; }

        camera = EntityManagerService.World.CreateEntity();
        camera.Add(new Transform());
        camera.Add(new OrthographicCamera() { ClearColor = Color.SkyBlue, IsMain = true, ViewSize = 10 });

        EntityRef testEntity = EntityManagerService.World.CreateEntity();
        testEntity.Add(new Transform() { Position = new Vector3(0, 0, 0), Scale = new Vector3(1, 1, 1) });
        testEntity.Add(new SpriteRenderer() { Sprite = new Sprite(Texture2D.White), Color = Color.Red });
    }

    public void OnUpdate(float deltaTime)
    {
        time += deltaTime;

        // Move the camera around circle with radius 5
        if (camera.Has<Transform>())
        {
            camera.Get<Transform>().Position = new Vector3(MathF.Cos(time) * 5, MathF.Sin(time) * 5, 0);
        }

        if (camera.Has<OrthographicCamera>())
        {
            camera.Get<OrthographicCamera>().ViewSize = 10 + MathF.Sin(time) * 2;
        }
    }
}
