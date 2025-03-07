using Arch.Core;
using Arch.Core.Extensions;
using Silk.NET.SDL;
using System.Numerics;

namespace MalignEngine
{
    public class CameraSystem : EntitySystem, IWindowDraw
    {
        [Dependency]
        protected ScheduleManager ScheduleManager = default!;
        [Dependency]
        protected WindowService Window = default!;
        [Dependency]
        protected IRenderingService Renderer = default!;

        public override void OnInitialize()
        {
        }

        public void OnWindowDraw(float delta)
        {
            if (Window.Width == 0 || Window.Height == 0)
            {
                return;
            }

            List<EntityRef> cameraEntities = new List<EntityRef>();
            EntityRef mainCamera = default;

            var query = new QueryDescription().WithAll<OrthographicCamera, Transform>();
            EntityManager.World.Query(in query, (EntityRef entity, ref OrthographicCamera camera, ref Transform transform) =>
            {
                if (camera.RenderTexture == null)
                {
                    camera.RenderTexture = new RenderTexture(Window.Width, Window.Height);
                }
                else if (camera.IsMain)
                {
                    camera.RenderTexture.Resize(Window.Width, Window.Height);
                }

                camera.Matrix = CreateOrthographicMatrix(camera.RenderTexture.Width, camera.RenderTexture.Height, camera.ViewSize, transform.Position.ToVector2());

                cameraEntities.Add(entity);

                if (camera.IsMain)
                {
                    mainCamera = entity;
                }
            });

            foreach (var cameraEntity in cameraEntities)
            {
                OrthographicCamera camera = cameraEntity.Get<OrthographicCamera>();

                Renderer.SetMatrix(camera.Matrix);

                Renderer.SetRenderTarget(camera.RenderTexture, camera.RenderTexture.Width, camera.RenderTexture.Height);
                Renderer.Clear(camera.ClearColor);
                Renderer.FlipY = true;
                ScheduleManager.Run<IDraw>(e => e.OnDraw(delta));
                Renderer.FlipY = false;

                if (camera.PostProcessingSteps != null)
                {
                    for (int i = 0; i < camera.PostProcessingSteps.Length; i++)
                    {
                        camera.PostProcessingSteps[i].Process(camera.RenderTexture);
                    }
                }
            }

            Renderer.SetRenderTarget(null);
            if (mainCamera != EntityRef.Null)
            {
                OrthographicCamera camera = mainCamera.Get<OrthographicCamera>();
                Renderer.Clear(camera.ClearColor);

                Renderer.SetMatrix(camera.Matrix);

                Renderer.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f));
                Renderer.DrawTexture2D(camera.RenderTexture, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Color.White, 0f, 0f);
                Renderer.End();
            }
            else
            {
                Renderer.Clear(Color.Black);
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
            if (camera.RenderTexture == null) { return new Vector2(); }

            position = new Vector2((position.X / camera.RenderTexture.Width - 0.5f) * 2f, (-position.Y / camera.RenderTexture.Height + 0.5f) * 2f);

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
}