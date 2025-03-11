using Arch.Core;
using Arch.Core.Extensions;
using ImGuiNET;
using System.Numerics;

namespace MalignEngine
{
    public class EditorSceneViewSystem : BaseEditorWindowSystem
    {
        public override string WindowName => "Scene View";

        public Vector2 WorldMousePosition { get; private set; }
        public bool IsWindowHovered { get; private set; }

        [Dependency]
        protected IRenderer2D IRenderingService = default!;
        [Dependency]
        protected TransformSystem TransformSystem = default!;
        [Dependency]
        protected InputSystem InputSystem = default!;
        [Dependency]
        protected CameraSystem CameraSystem = default!;

        private EntityRef camera;

        public override void OnInitialize()
        {
            base.OnInitialize();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!IsWindowHovered) { return; }

            if (InputSystem.IsMouseButtonPressed(1))
            {
                Vector2 delta = InputSystem.MouseDelta * deltaTime;
                delta.X = -delta.X;
                camera.Get<Transform>().Position += delta.ToVector3() * camera.Get<OrthographicCamera>().ViewSize * 0.25f;
            }

            if (InputSystem.IsMouseButtonPressed(0))
            {
                var query = new QueryDescription().WithAll<Transform>();
                EntityManager.World.Query(in query, (EntityRef entity, ref Transform transform) =>
                {
                    if (WorldMousePosition.X > transform.Position.X - transform.Scale.X / 2 && WorldMousePosition.X < transform.Position.X + transform.Scale.X / 2)
                    {
                        if (WorldMousePosition.Y > transform.Position.Y - transform.Scale.Y / 2 && WorldMousePosition.Y < transform.Position.Y + transform.Scale.Y / 2)
                        {
                            EditorSystem.SelectedEntity = entity;
                        }
                    }
                });
            }

            camera.Get<OrthographicCamera>().ViewSize -= InputSystem.MouseScroll * deltaTime * 50f;
        }

        public override void DrawWindow(float deltaTime)
        {
            if (!camera.IsValid())
            {
                camera = EntityManager.World.CreateEntity();
                camera.Add(new Transform());
                camera.Add(new OrthographicCamera() { ViewSize = 6f, IsMain = false, ClearColor = Color.Black });
            }

            ImGui.Begin("Scene View");

            if (ImGui.IsWindowHovered())
            {
                IsWindowHovered = true;
                Vector2 mousePosition = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), InputSystem.MousePosition - ImGui.GetWindowPos() - ImGui.GetWindowContentRegionMin());

                WorldMousePosition = mousePosition;

                if (InputSystem.IsMouseButtonPressed(0))
                {
                    IRenderingService.Begin();
                    IRenderingService.DrawTexture2D(Texture2D.White, mousePosition, new Vector2(0.1f, 0.1f), Color.Red, 0f, 15);
                    IRenderingService.End();
                }
            }
            else
            {
                IsWindowHovered = false;
            }

            Vector2 size = ImGui.GetWindowSize() - new Vector2(20f, 40f);

            if (size.X < 1) { size.X = 1f; }
            if (size.Y < 1) { size.Y = 1f; }

            RenderTexture renderTexture = camera.Get<OrthographicCamera>().RenderTexture;

            if (renderTexture != null)
            {
                ImGuiSystem.Image(renderTexture, size, uv0: new Vector2(0, 1), uv1: new Vector2(1, 0));
                renderTexture.Resize((int)size.X, (int)size.Y);
            }

            ImGui.End();
        }
    }
}