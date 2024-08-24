using Arch.Core;
using Arch.Core.Extensions;
using Silk.NET.Input;
using Silk.NET.OpenGLES;
using System.Numerics;

namespace MalignEngine
{
    public class CameraSystem : EntitySystem
    {
        [Dependency]
        protected WindowSystem Window = default!;
        [Dependency]
        protected RenderingSystem Renderer = default!;

        private Matrix4x4 matrix;

        public override void Initialize()
        {
            Window.OnWindowDraw += RequestDraw;
        }

        public void RequestDraw(float delta)
        {
            if (Window.Width == 0 || Window.Height == 0)
            {
                return;
            }

            List<Entity> cameraEntities = new List<Entity>();
            Entity mainCamera = Entity.Null;

            var query = new QueryDescription().WithAll<OrthographicCamera, Position2D>();
            World.Query(in query, (Entity entity, ref OrthographicCamera camera, ref Position2D position) =>
            {
                if (camera.RenderTexture == null)
                {
                    camera.RenderTexture = new RenderTexture((uint)Window.Width, (uint)Window.Height);
                }
                else if (camera.IsMain)
                {
                    camera.RenderTexture.Resize((uint)Window.Width, (uint)Window.Height);
                }

                camera.Matrix = CreateOrthographicMatrix(camera.RenderTexture.Width, camera.RenderTexture.Height, camera.ViewSize, position.Position);

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
                Renderer.Clear(Color.LightSkyBlue);
                SystemGroup.Draw(delta);
            }

            Renderer.SetRenderTarget(null);
            Renderer.Clear(Color.LightSkyBlue);

            if (mainCamera != Entity.Null)
            {
                OrthographicCamera camera = mainCamera.Get<OrthographicCamera>();

                Renderer.Begin(camera.PostProcessing, Matrix4x4.CreateOrthographicOffCenter(0f, Window.Width, Window.Height, 0, 0.001f, 100f));
                Renderer.DrawRenderTexture(camera.RenderTexture, new Vector2(Window.Width / 2f, Window.Height / 2f), new Vector2(Window.Width, Window.Height), Vector2.Zero, new Rectangle(0, 0, 800, 600), Color.White, 0f, 0f);
                Renderer.End();
            }

            SystemGroup.DrawGUI(delta);

        }

        public Matrix4x4 CreateOrthographicMatrix(float width, float height, float viewSize, Vector2 position)
        {
            float aspectRatio = width / height;
            var left = viewSize * -aspectRatio + position.X;
            var right = viewSize * aspectRatio + position.X;
            var top = viewSize * -1.0f + position.Y;
            var bottom = viewSize * 1.0f + position.Y;

            var orthographicMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, top, bottom, 0.001f, 100f);

            return orthographicMatrix;
        }

        public Vector2 ScreenToWorld(ref OrthographicCamera camera, Vector2 position)
        {
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