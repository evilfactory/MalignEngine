using Arch.Core;
using Arch.Core.Extensions;
using Silk.NET.SDL;
using System.Numerics;

namespace MalignEngine;

public interface ICameraDraw : ISchedule
{
    public void OnCameraDraw(float delta, OrthographicCamera camera);
}

public class CameraSystem : BaseSystem
{
    private IScheduleManager _scheduleManager;
    private IWindowService _windowService;
    private IRenderer2D _renderer2D;
    private IRenderingAPI _renderApi;
    private IEntityManager _entityManager;
    private IEventService _eventService;

    public CameraSystem(ILoggerService loggerService, IScheduleManager scheduleManager, IEntityManager entityManager, IEventService eventService, IRenderingAPI renderApi, IWindowService windowService, IRenderer2D renderer2D)
        : base(loggerService, scheduleManager)
    {
        _renderApi = renderApi;
        _windowService = windowService;
        _renderer2D = renderer2D;
        _scheduleManager = scheduleManager;
        _entityManager = entityManager;
        _eventService = eventService;

        _eventService.Get<ComponentEventChannel<ComponentAddedEvent>>().Subscribe<OrthographicCamera>((entity, addedEvent) =>
        {
            entity.Get<OrthographicCamera>().Output = _renderApi.CreateFrameBuffer(new FrameBufferDescriptor(1, 512, 512));
        });
    }

    public override void OnDraw(float delta)
    {
        if (_windowService.FrameSize.X == 0 || _windowService.FrameSize.Y == 0)
        {
            return;
        }

        List<EntityRef> cameraEntities = new List<EntityRef>();
        EntityRef mainCamera = default;

        var query = new QueryDescription().WithAll<OrthographicCamera, Transform>();
        _entityManager.World.Query(in query, (EntityRef entity, ref OrthographicCamera camera, ref Transform transform) =>
        {
            if (camera.Output == null)
            {
                return;
            }

            if (camera.IsMain)
            {
                camera.Output.Resize(_windowService.FrameSize.X, _windowService.FrameSize.Y);
            }

            camera.Matrix = CreateOrthographicMatrix(camera.Output.Width, camera.Output.Height, camera.ViewSize, transform.Position.ToVector2());

            cameraEntities.Add(entity);

            if (camera.IsMain)
            {
                mainCamera = entity;
            }
        });

        foreach (var cameraEntity in cameraEntities)
        {
            OrthographicCamera camera = cameraEntity.Get<OrthographicCamera>();

            _renderApi.Submit(ctx =>
            {
                _renderer2D.SetMatrix(camera.Matrix);

                ctx.SetFrameBuffer(camera.Output, camera.Output.Width, camera.Output.Height);
                ctx.Clear(camera.ClearColor);
            });

            _scheduleManager.Run<ICameraDraw>(e => e.OnCameraDraw(delta, camera));

            if (camera.PostProcessingSteps != null)
            {
                for (int i = 0; i < camera.PostProcessingSteps.Length; i++)
                {
                    camera.PostProcessingSteps[i].Process(camera.Output);
                }
            }
        }

        if (mainCamera != EntityRef.Null)
        {
            OrthographicCamera camera = mainCamera.Get<OrthographicCamera>();

            _renderApi.Submit(ctx =>
            {
                ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);

                ctx.Clear(Color.Black);

                _renderer2D.Begin(ctx, Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f));
                _renderer2D.DrawTexture2D(camera.Output.GetColorAttachment(0), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Color.White, 0f, 0f);
                _renderer2D.End();
            });

        }
        else
        {
            _renderApi.Submit(ctx =>
            {
                ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);

                ctx.Clear(Color.Black);
            });
        }
    }

    public Matrix4x4 CreateOrthographicMatrix(float width, float height, float viewSize, Vector2 position)
    {
        float aspectRatio = width / height;
        var left = viewSize * -aspectRatio + position.X;
        var right = viewSize * aspectRatio + position.X;
        var top = viewSize * -1.0f + position.Y;
        var bottom = viewSize * 1.0f + position.Y;

        return Matrix4x4.CreateOrthographicOffCenter(left, right, top, bottom, 0.001f, 100f);
    }

    public Vector2 ScreenToWorld(ref OrthographicCamera camera, Vector2 position)
    {
        if (camera.Output == null) { return new Vector2(); }

        position = new Vector2((position.X / camera.Output.Width - 0.5f) * 2f, (-position.Y / camera.Output.Height + 0.5f) * 2f);

        Matrix4x4.Invert(camera.Matrix, out Matrix4x4 invMatrix);

        position = Vector2.Transform(position, invMatrix);

        return position;
    }

    public Vector2 WorldToScreen(ref OrthographicCamera camera, Vector2 position)
    {
        position = Vector2.Transform(position, camera.Matrix);
        return position;
    }
}