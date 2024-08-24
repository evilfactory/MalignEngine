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
        protected InputSystem InputSystem = default!;
        [Dependency]
        protected CameraSystem CameraSystem = default!;

        private Entity camera;

        public override void Initialize()
        {
            base.Initialize();

            camera = World.Create(new OrthographicCamera { ViewSize = 6f, IsMain = false, RenderTexture = new RenderTexture(800, 600) }, new Position2D { Position = new Vector2(0, 0) });
        }

        public override void Draw(float deltaTime)
        {
            if (!Active) { return; }

            ImGui.Begin("Scene View");

            if (ImGui.IsWindowHovered())
            {
                if (InputSystem.IsMouseButtonPressed(1))
                {
                    Vector2 delta = InputSystem.MouseDelta * deltaTime;
                    delta.X = -delta.X;
                    camera.Get<Position2D>().Position += delta;
                }

                if (InputSystem.IsMouseButtonPressed(0))
                {
                    var query = new QueryDescription().WithAll<Position2D>();
                    World.Query(in query, (Entity entity, ref Position2D position) =>
                    {
                        Vector2 mousePosition = CameraSystem.ScreenToWorld(ref camera.Get<OrthographicCamera>(), InputSystem.MousePosition);

                        if (entity.TryGet(out Scale2D scale))
                        {
                            if (mousePosition.X > position.Position.X - scale.Scale.X / 2 && mousePosition.X < position.Position.X + scale.Scale.X / 2)
                            {
                                if (mousePosition.Y > position.Position.Y - scale.Scale.Y / 2 && mousePosition.Y < position.Position.Y + scale.Scale.Y / 2)
                                {
                                    EditorSystem.SelectedEntity = entity.Reference();
                                }
                            }
                        }
                        else
                        {
                            if (Vector2.Distance(mousePosition, position.Position) < 1f)
                            {
                                EditorSystem.SelectedEntity = entity.Reference();
                            }
                        }
                    });
                }

                camera.Get<OrthographicCamera>().ViewSize -= InputSystem.MouseScroll * deltaTime * 10f;
            }

            Vector2 size = ImGui.GetWindowSize() - new Vector2(20f, 40f);

            camera.Get<OrthographicCamera>().RenderTexture.Resize((uint)size.X, (uint)size.Y);

            ImGuiSystem.Image(camera.Get<OrthographicCamera>().RenderTexture, size, uv0: new Vector2(0, 1), uv1: new Vector2(1, 0));

            ImGui.End();
        }
    }
}