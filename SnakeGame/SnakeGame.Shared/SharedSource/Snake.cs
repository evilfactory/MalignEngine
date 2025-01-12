using System.Numerics;
using MalignEngine;

namespace SnakeGame;

public class SnakeSystem : EntitySystem
{
#if CLIENT
    [Dependency]
    protected RenderingSystem RenderingSystem = default!;
#endif

    public override void OnInitialize()
    {
        EntityEventSystem.SubscribeEvent<SnakeComponent, ComponentInitEvent>(OnSnakeComponentInit);
    }

    public override void OnUpdate(float deltaTime)
    {

    }

#if CLIENT
    public override void OnDraw(float deltaTime)
    {
        EntityManager.World.Query(EntityManager.World.CreateQuery().WithAll<SnakeComponent, Transform>(), (EntityRef entity, ref SnakeComponent snakeComponent, ref Transform transform) =>
        {
            RenderingSystem.Begin();
            RenderingSystem.DrawTexture2D(Texture2D.White, transform.Position.ToVector2(), Vector2.One, 0f);
            
            for (int i = 0; i < snakeComponent.Tail.Count; i++)
            {
                RenderingSystem.DrawTexture2D(Texture2D.White, transform.Position.ToVector2(), Vector2.One, Color.Red, 0f, 0f);
            }

            RenderingSystem.End();
        });
    }
#endif

    public void OnSnakeComponentInit(EntityRef entity, SnakeComponent snakeComponent)
    {
        entity.Get<SnakeComponent>().Tail = new List<Vector2>();
    }
}

public struct SnakeComponent : IComponent
{
    public byte InputDirection;
    public List<Vector2> Tail;
}