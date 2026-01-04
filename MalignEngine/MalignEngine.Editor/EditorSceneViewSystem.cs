using ImGuiNET;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MalignEngine.Editor;

public class EditorSceneViewSystem : BaseEditorWindowSystem
{
    public override string WindowName => "Scene View";

    public Vector2 WorldMousePosition { get; private set; }
    public bool IsWindowHovered { get; private set; }

    private IRenderer2D _renderer2D = default!;
    private TransformSystem _transformSystem = default!;
    private IInputService _inputService = default!;
    private CameraSystem _cameraSystem = default!;
    private IEntityManager _entityManager;

    private Entity _camera;

    public EditorSceneViewSystem(ILoggerService loggerService, IScheduleManager scheduleManager, EditorSystem editorSystem, ImGuiSystem imGuiService, IEntityManager entityManager, IRenderer2D renderer2D, TransformSystem transformSystem, IInputService inputService, CameraSystem cameraSystem) 
        : base(loggerService, scheduleManager, editorSystem, imGuiService)
    {
        _renderer2D = renderer2D;
        _transformSystem = transformSystem;
        _inputService = inputService;
        _cameraSystem = cameraSystem;
        _entityManager = entityManager;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (!IsWindowHovered) { return; }

        if (_inputService.Mouse.IsButtonPressed(MouseButton.Right))
        {
            Vector2 delta = _inputService.Mouse.Delta * deltaTime;
            delta.X = -delta.X;
            _camera.Get<Transform>().Position += delta.ToVector3() * _camera.Get<OrthographicCamera>().ViewSize * 0.25f;
        }

        if (_inputService.Mouse.IsButtonPressed(MouseButton.Left))
        {
            _entityManager.World.Query(new Query().Include<Transform>(), (Entity entity) =>
            {
                ref Transform transform = ref entity.Get<Transform>();
                if (WorldMousePosition.X > transform.Position.X - transform.Scale.X / 2 && WorldMousePosition.X < transform.Position.X + transform.Scale.X / 2)
                {
                    if (WorldMousePosition.Y > transform.Position.Y - transform.Scale.Y / 2 && WorldMousePosition.Y < transform.Position.Y + transform.Scale.Y / 2)
                    {
                        EditorSystem.SelectedEntity = entity;
                    }
                }
            });
        }

        _camera.Get<OrthographicCamera>().ViewSize -= _inputService.Mouse.ScrollDelta * deltaTime * 50f;
    }

    public override void DrawWindow(float deltaTime)
    {
        if (!_camera.IsAlive())
        {
            _camera = _entityManager.World.CreateEntity();
            _camera.AddOrSet(new Transform());
            _camera.AddOrSet(new OrthographicCamera() { ViewSize = 6f, IsMain = false, ClearColor = Color.Black });
        }

        ImGui.Begin("Scene View");

        if (ImGui.IsWindowHovered())
        {
            IsWindowHovered = true;
            Vector2 mousePosition = _cameraSystem.ScreenToWorld(ref _camera.Get<OrthographicCamera>(), _inputService.Mouse.Position - ImGui.GetWindowPos() - ImGui.GetWindowContentRegionMin());

            WorldMousePosition = mousePosition;

            if (_inputService.Mouse.IsButtonPressed(MouseButton.Left))
            {
                //_renderer2D.Begin();
                //_renderer2D.DrawTexture2D(Texture2D.White, mousePosition, new Vector2(0.1f, 0.1f), Color.Red, 0f, 15);
                //_renderer2D.End();
            }
        }
        else
        {
            IsWindowHovered = false;
        }

        Vector2 size = ImGui.GetWindowSize() - new Vector2(20f, 40f);

        if (size.X < 1) { size.X = 1f; }
        if (size.Y < 1) { size.Y = 1f; }

        if (_camera.TryGet(out ComponentRef<CameraRenderData> renderData))
        {
            IFrameBufferResource renderTexture = renderData.Value.Output;

            if (renderTexture != null)
            {
                ImGuiService.Image(renderTexture.GetColorAttachment(0), size, uv0: new Vector2(0, 1), uv1: new Vector2(1, 0));
                renderTexture.Resize((int)size.X, (int)size.Y);
            }
        }

        ImGui.End();
    }
}