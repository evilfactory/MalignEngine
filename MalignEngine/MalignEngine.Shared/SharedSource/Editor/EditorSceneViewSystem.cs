using Arch.Core;
using Arch.Core.Extensions;
using ImGuiNET;
using System.Numerics;

namespace MalignEngine
{
    public class EditorSceneViewSystem : BaseEditorWindowSystem
    {
        public override string WindowName => "Scene View";

        [Dependency]
        protected RenderingSystem RenderingSystem = default!;
        [Dependency]
        protected TransformSystem TransformSystem = default!;
        [Dependency]
        protected InputSystem InputSystem = default!;
        [Dependency]
        protected CameraSystem CameraSystem = default!;

        private Entity camera;

        public override void OnInitialize()
        {
            base.OnInitialize();

            camera = World.Create(new OrthographicCamera { ViewSize = 6f, IsMain = false, RenderTexture = new RenderTexture(800, 600) }, new Transform(new Vector2(0, 0)));
        }

        public override void OnDraw(float deltaTime)
        {
            if (!Active) { return; }

            ImGui.Begin("Scene View");

            if (ImGui.IsWindowHovered())
            {
                if (InputSystem.IsMouseButtonPressed(1))
                {
                    Vector2 delta = InputSystem.MouseDelta * deltaTime;
                    delta.X = -delta.X;
                    camera.Get<Transform>().Position += delta.ToVector3();
                }

                if (InputSystem.IsMouseButtonPressed(0))
                {
                    var query = new QueryDescription().WithAll<Transform>();
                    World.Query(in query, (Entity entity, ref Transform transform) =>
                    {
                        Vector2 mousePosition = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), InputSystem.MousePosition - ImGui.GetWindowPos() - ImGui.GetWindowContentRegionMin());

                        RenderingSystem.Begin();
                        RenderingSystem.DrawTexture2D(Texture2D.White, mousePosition, new Vector2(0.1f, 0.1f), Vector2.Zero, new Rectangle(), Color.Red, 0f, 15);
                        RenderingSystem.End();

                        if (mousePosition.X > transform.Position.X - transform.Scale.X / 2 && mousePosition.X < transform.Position.X + transform.Scale.X / 2)
                        {
                            if (mousePosition.Y > transform.Position.Y - transform.Scale.Y / 2 && mousePosition.Y < transform.Position.Y + transform.Scale.Y / 2)
                            {
                                EditorSystem.SelectedEntity = entity.Reference();
                            }
                        }
                    });
                }

                camera.Get<OrthographicCamera>().ViewSize -= InputSystem.MouseScroll * deltaTime * 10f;
            }

            Vector2 size = ImGui.GetWindowSize() - new Vector2(20f, 40f);

            if (size.X < 1) { size.X = 1f; }
            if (size.Y < 1) { size.Y = 1f; }

            camera.Get<OrthographicCamera>().RenderTexture.Resize((uint)size.X, (uint)size.Y);

            ImGuiSystem.Image(camera.Get<OrthographicCamera>().RenderTexture, size, uv0: new Vector2(0, 1), uv1: new Vector2(1, 0));

            ImGui.End();
        }
    }
}