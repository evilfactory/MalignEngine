using Silk.NET.SDL;
using System.Numerics;

namespace MalignEngine;

public interface ICameraDraw : ISchedule
{
    public void OnCameraDraw(float delta, OrthographicCamera camera);
}

public struct CameraRenderData : IComponent
{
    public IFrameBufferResource Output;
}

public class CameraSystem : EntitySystem
{
    private IWindowService _windowService;
    private IRenderer2D _renderer2D;
    private IRenderingAPI _renderApi;

    public CameraSystem(IServiceContainer serviceContainer, IWindowService windowService, IRenderingAPI renderApi, IRenderer2D renderer2D)
        : base(serviceContainer)
    {
        _renderApi = renderApi;
        _windowService = windowService;
        _renderer2D = renderer2D;
    }

    public override void OnDraw(float delta)
    {
        if (_windowService.FrameSize.X == 0 || _windowService.FrameSize.Y == 0)
        {
            return;
        }

        List<Entity> cameraEntities = new List<Entity>();
        Entity mainCamera = default;

        World.Query(new Query().WithAll<OrthographicCamera>().Exclude<CameraRenderData>(), (Entity entity) =>
        {
            entity.AddOrSet(new CameraRenderData()
            {
                Output = _renderApi.CreateFrameBuffer(new FrameBufferDescriptor(1, 512, 512))
            });
        });

        World.Query(new Query().WithAll<OrthographicCamera, CameraRenderData, Transform>(), (Entity entity) =>
        {
            ref OrthographicCamera camera = ref entity.Get<OrthographicCamera>();
            ref CameraRenderData renderData = ref entity.Get<CameraRenderData>();
            ref Transform transform = ref entity.Get<Transform>();

            if (renderData.Output == null)
            {
                return;
            }

            if (camera.IsMain)
            {
                renderData.Output.Resize(_windowService.FrameSize.X, _windowService.FrameSize.Y);
                camera.Width = _windowService.FrameSize.X;
                camera.Height = _windowService.FrameSize.Y;
            }

            camera.Matrix = CreateOrthographicMatrix(renderData.Output.Width, renderData.Output.Height, camera.ViewSize, transform.Position.ToVector2());

            cameraEntities.Add(entity);

            if (camera.IsMain)
            {
                mainCamera = entity;
            }
        });

        foreach (var cameraEntity in cameraEntities)
        {
            OrthographicCamera camera = cameraEntity.Get<OrthographicCamera>();
            CameraRenderData renderData = cameraEntity.Get<CameraRenderData>();

            _renderApi.Submit(ctx =>
            {
                _renderer2D.SetMatrix(camera.Matrix);

                ctx.SetFrameBuffer(renderData.Output, renderData.Output.Width, renderData.Output.Height);
                ctx.Clear(camera.ClearColor);
            });

            ScheduleManager.Run<ICameraDraw>(e => e.OnCameraDraw(delta, camera));

            if (camera.PostProcessingSteps != null)
            {
                for (int i = 0; i < camera.PostProcessingSteps.Length; i++)
                {
                    camera.PostProcessingSteps[i].Process(renderData.Output);
                }
            }
        }

        if (mainCamera != Entity.Null)
        {
            OrthographicCamera camera = mainCamera.Get<OrthographicCamera>();
            CameraRenderData renderData = mainCamera.Get<CameraRenderData>();

            _renderApi.Submit(ctx =>
            {
                ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);

                ctx.Clear(Color.Black);

                _renderer2D.Begin(ctx, Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f));
                _renderer2D.DrawTexture2D(renderData.Output.GetColorAttachment(0), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Color.White, 0f, 0f);
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
        position = new Vector2((position.X / camera.Width - 0.5f) * 2f, (-position.Y / camera.Height + 0.5f) * 2f);

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