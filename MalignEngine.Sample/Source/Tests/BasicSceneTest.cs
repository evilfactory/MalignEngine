using MalignEngine.Sample;
using System.Numerics;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace MalignEngine;

public class BasicSceneTest : IService, IUpdate, IStateEnter<GameState>
{
    [Dependency]
    protected EntityManager EntityManager = default!;
    [Dependency]
    protected IInputService InputSystem = default!;
    [Dependency]
    protected CameraSystem CameraSystem = default!;
    [Dependency]
    protected SceneSystem SceneSystem = default!;
    [Dependency]
    protected PhysicsSystem2D PhysicsSystem = default!;

    private Scene physicsObject;

    private EntityRef camera;

    private void SpawnPhysicsObject(Vector3 position)
    {
        EntityRef entity = SceneSystem.Instantiate(physicsObject);
        PhysicsSystem.SetPosition(entity, position.ToVector2());
    }

    public void OnStateEnter(GameState state)
    {
        if (state != GameState.BasicSceneTest) { return; }

        physicsObject = new Scene();

        camera = EntityManager.World.CreateEntity();
        camera.Add(new Transform());
        camera.Add(new OrthographicCamera() { ClearColor = Color.SkyBlue, IsMain = true, ViewSize = 10 });

        EntityRef entity = physicsObject.Root;
        entity.Add(new Transform() { Position = new Vector3(), Scale = new Vector3(1, 1, 1) });
        entity.Add(new SpriteRenderer() { Sprite = new Sprite(Texture2D.White), Color = Color.Red });
        entity.Add(new PhysicsBody2D()
        {
            Fixtures = new FixtureData2D[]
            {
                new FixtureData2D(new RectangleShape2D(1, 1), 1, 0.2f, 0.5f)
            },
            BodyType = PhysicsBodyType.Dynamic
        });

        EntityRef floor = EntityManager.World.CreateEntity();
        floor.Add(new Transform() { Position = new Vector3(0, -7, 0), Scale = new Vector3(100, 1, 1) });
        floor.Add(new SpriteRenderer() { Sprite = new Sprite(Texture2D.White), Color = Color.Green });
        floor.Add(new PhysicsBody2D()
        {
            Fixtures = new FixtureData2D[]
            {
                new FixtureData2D(new RectangleShape2D(100, 1), 0, 0.2f, 0.5f)
            },
            BodyType = PhysicsBodyType.Static
        });

        for (int i = -5; i < 5; i++)
        {
            SpawnPhysicsObject(new Vector3(i, 0, 0));
        }
    }

    public void OnUpdate(float deltaTime)
    {
        if (InputSystem.IsMouseButtonPressed(0))
        {
            Vector2 mousePosition = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), InputSystem.MousePosition);

            SpawnPhysicsObject(mousePosition.ToVector3());
        }
    }
}