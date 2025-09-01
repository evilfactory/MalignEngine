using System.Runtime.InteropServices;
using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine.Experimentation;

class Experimentation : IService, IDraw, ICameraDraw
{
    private ILogger _logger;
    private IRenderingAPI _renderAPI;
    private IRenderer2D _render2D;
    private IFontRenderer _fontRenderer;
    private IWindowService _windowService;
    private IInputService _inputService;
    private IEntityManager _entityManager;
    private IAssetService _assetService;

    private IShaderResource _shaderResource;
    private IShaderResource _shaderResource2;
    private ITextureResource _textureResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IFrameBufferResource _frameBufferResource;

    private SceneSystem _sceneSystem;
    private SceneXmlLoader _sceneXmlLoader;

    private Font _font;

    public Experimentation(
        ILoggerService loggerService,
        IRenderingAPI renderAPI,
        IAssetService assetService,
        IRenderer2D render2D,
        IWindowService windowService,
        IFontRenderer fontRenderer,
        IInputService inputService,
        IEntityManager entityManager,
        SceneXmlLoader sceneXmlLoader,
        SceneSystem sceneSystem,
        ITileSystem tileSystem
        )
    {
        _logger = loggerService.GetSawmill("experimentation");
        _renderAPI = renderAPI;
        _render2D = render2D;
        _windowService = windowService;
        _fontRenderer = fontRenderer;
        _inputService = inputService;
        _entityManager = entityManager;
        _assetService = assetService;
        _sceneXmlLoader = sceneXmlLoader;
        _sceneSystem = sceneSystem;

        tileSystem.CreateTileMap(new List<TileLayer>() { new TileLayer("Wall", 0, true) });

        _shaderResource = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/TestFrag.glsl"),
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl")
        });

        _shaderResource2 = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/TestFrag2.glsl"),
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl")
        });

        _textureResource = _renderAPI.CreateTexture(TextureLoader.Load("Content/Textures/player.png"));

        var desc = new VertexArrayDescriptor();
        desc.AddAttribute("Position", 0, VertexAttributeType.Float, 3, false);
        desc.AddAttribute("UV", 1, VertexAttributeType.Float, 2, false);
        _vertexArrayResource = _renderAPI.CreateVertexArray(desc);

        float[] imageData = new float[]
        {
            -1, -1, 0f,     0f, 0f, // Bottom-left
             1, -1, 0f,     1f, 0f, // Bottom-right
             1,  1, 0f,     1f, 1f, // Top-right

            // Triangle 2
            -1, -1, 0f,     0f, 0f, // Bottom-left
             1,  1, 0f,     1f, 1f, // Top-right
            -1,  1, 0f,     0f, 1f  // Top-left
        };

        _bufferResource = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Vertex, BufferUsageType.Static, MemoryMarshal.AsBytes(imageData.AsSpan()).ToArray()));

        _frameBufferResource = _renderAPI.CreateFrameBuffer(new FrameBufferDescriptor(1, 1280, 800));

        _font = _assetService.FromPath<Font>("file:Content/Roboto-Regular.ttf");
        _inputService = inputService;
        _entityManager = entityManager;

        /*
        AssetHandle<Sprite> sprite = _assetService.FromPath<Sprite>("file:Content/FooSprite.xml");

        EntityRef camera = _entityManager.World.CreateEntity();
        camera.Add(new Transform()
        {
            Scale = Vector3.One
        });
        camera.Add(new OrthographicCamera()
        {
            IsMain = true,
            ViewSize = 20f,
            ClearColor = Color.BlueViolet,
        });
        camera.Add(new SpriteRenderer()
        {
            Color = Color.Red,
            Sprite = sprite
        });

        Scene scene = new Scene("test");

        scene.CopyEntities(new EntityRef[] { camera });

        XElement test = new XElement("Scene");
        sceneXmlLoader.Save(test, scene);
        _logger.LogInfo(test.ToString());
        */

        assetService.PreLoad("Content");

        //var asset = assetService.FromPath<TileList>("file:Content/TileList.xml").Asset;

        AssetHandle<Scene> scene = _assetService.FromPath<Scene>("file:Content/FooScene.xml");
        EntityRef entity = _sceneSystem.Instantiate(scene);
        assetService.FromAsset(new Texture2D(entity.Get<OrthographicCamera>().Output.GetColorAttachment(0)));
    }

    public void OnCameraDraw(float delta, OrthographicCamera camera)
    {
        return;

        _renderAPI.Submit(ctx =>
        {
            _render2D.Begin(ctx);
            _render2D.DrawTexture2D(_textureResource, new Vector2(-10f, -3f), new Vector2(15f, 15f), 0f);
            _render2D.End();
        });
    }

    public void OnDraw(float deltaTime)
    {
        return;

        var matrix = Matrix4x4.CreateOrthographicOffCenter(0, _windowService.FrameSize.X, _windowService.FrameSize.Y, 0, 0.0001f, 100f);

        _renderAPI.Submit((IRenderContext ctx) =>
        {
            ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);
            ctx.Clear(Color.LightGray);

            _render2D.SetMatrix(matrix);


            _render2D.Begin(ctx);

            _fontRenderer.DrawFont(_font, 120, "hello wawawawawawa", _inputService.Mouse.Position, Color.Red);

            Vector2 scale = new Vector2(_windowService.FrameSize.X / 32f, _windowService.FrameSize.X / 32f);

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    _render2D.DrawTexture2D(_textureResource, _inputService.Mouse.Position + new Vector2(x * scale.X, y * scale.Y), new Vector2(scale.X, scale.Y), 0f);
                }
            }
            _render2D.End();

            /*
            ctx.SetShader(_shaderResource);
            ctx.SetTexture(0, _textureResource);
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);

            ctx.SetFrameBuffer(null, 1280, 800);
            ctx.Clear(Color.Green);
            ctx.SetShader(_shaderResource2);
            ctx.SetTexture(0, _frameBufferResource.GetColorAttachment(0));
            ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);
            */
        });

    }
}
