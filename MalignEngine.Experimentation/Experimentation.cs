using System.Runtime.InteropServices;
using System.Numerics;
using System.Xml.Linq;
using SixLabors.ImageSharp;

namespace MalignEngine.Experimentation;

class Experimentation : EntitySystem, ICameraDraw
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
    private CameraSystem _cameraSystem;

    private Font _font;

    private Entity entity;

    public Experimentation(
        IServiceContainer serviceContainer,
        IRenderingAPI renderAPI,
        IAssetService assetService,
        IRenderer2D render2D,
        IWindowService windowService,
        IFontRenderer fontRenderer,
        IInputService inputService,
        IEntityManager entityManager,
        SceneXmlLoader sceneXmlLoader,
        SceneSystem sceneSystem,
        CameraSystem cameraSystem,
        UIManager uiManager
        )
        : base(serviceContainer)
    {
        _renderAPI = renderAPI;
        _render2D = render2D;
        _windowService = windowService;
        _fontRenderer = fontRenderer;
        _inputService = inputService;
        _entityManager = entityManager;
        _assetService = assetService;
        _sceneXmlLoader = sceneXmlLoader;
        _sceneSystem = sceneSystem;
        _cameraSystem = cameraSystem;

        //tileSystem.CreateTileMap(new List<TileLayer>() { new TileLayer("Wall", 0, true) });

        assetService.Mount("/Content/", new FileAssetSource("Content"));
        var httpClient = new HttpClient();

        _shaderResource = _assetService.FromPath<ShaderAsset>("/Content/TestShader.shader").Asset.ShaderResource;

        //_textureResource = _renderAPI.CreateTexture(TextureLoader.Load("Content/Textures/player.png"));
        _textureResource = _assetService.FromPath<Texture2D>("/Content/Textures/player.png").Asset.Resource;

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

        _font = _assetService.FromPath<Font>("/Content/Roboto-Regular.ttf");
        _inputService = inputService;
        _entityManager = entityManager;

        AssetHandle<Sprite> sprite = _assetService.FromPath<Sprite>("/Content/FooSprite.xml");

        /*
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

        //var asset = assetService.FromPath<TileList>("file:Content/TileList.xml").Asset;

        AssetHandle<Scene> scene = _assetService.FromPath<Scene>("/Content/FooScene.xml");
        entity = _sceneSystem.Instantiate(scene);
        //assetService.FromAsset(new Texture2D(entity.Get<OrthographicCamera>().Output.GetColorAttachment(0)));

        Theme theme = new Theme();

        theme.Add("text", new TextStyle() { Font = _font, FontSize = 35, TextColor = Color.White });
        theme.Add("button", new ButtonStyle() 
        { 
            TextStyle = theme.Get<TextStyle>("text"), 
            ClickColor = Color.MediumVioletRed, 
            Color = Color.IndianRed, 
            HoverColor = Color.IndianRed * 0.8f 
        });

        theme.Add("textfield", new TextFieldStyle()
        {
            TextStyle = theme.Get<TextStyle>("text"),
            Color = Color.IndianRed,
            CursorColor = new Color(200, 200, 200, 150)
        });

        Border border = new Border()
        {
            Color = new Color(200, 200, 200, 50),
            Width = Length.Percent(1f),
            Height = Length.Percent(1f)
        };

        Stack stack = new Stack() 
        { 
            Width = Length.Percent(0.5f), 
            Height = Length.Percent(0.5f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 10f
        };

        TextBlock text = new TextBlock(theme.Get<TextStyle>("text"))
        {
            Width = Length.Percent(1f),
            Height = Length.Percent(0.2f),
            Text = "This is an experiment",
            FontSize = 100,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Button button = new Button(theme.Get<ButtonStyle>("button"))
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(60f),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisibile = true,
            Text = "Button1"
        };

        Button button2 = new Button(theme.Get<ButtonStyle>("button"))
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(60f),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisibile = true,
            Text = "Button2"
        };

        Button button3 = new Button(theme.Get<ButtonStyle>("button"))
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(60f),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisibile = true,
            Text = "Button3"
        };

        Button button4 = new Button(theme.Get<ButtonStyle>("button"))
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(60f),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisibile = true,
            Text = "Button3"
        };

        TextField textField = new TextField(theme.Get<TextFieldStyle>("textfield"))
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(60f),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisibile = true,
        };

        Flex flex = new Flex()
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(150f),
            Orientation = Orientation.Horizontal
        };

        Flex flex2 = new Flex()
        {
            Width = Length.Percent(0.5f),
            Height = Length.Percent(1f),
            Orientation = Orientation.Vertical
        };


        Border border1 = new Border()
        {
            Color = Color.Green,
            Width = Length.Percent(0.5f),
            Height = Length.Percent(1f)
        };

        Border border2 = new Border()
        {
            Color = Color.Red,
            Width = Length.Percent(1f),
            Height = Length.Percent(0.3f)
        };

        Border border3 = new Border()
        {
            Color = Color.Blue,
            Width = Length.Percent(1f),
            Height = Length.Fill
        };

        ImageBox image = new ImageBox()
        {
            Width = Length.Percent(1f),
            Height = Length.Pixels(100f),
            Sprite = sprite
        };

        border1.Parent = flex;
        border2.Parent = flex2;
        border3.Parent = flex2;

        flex2.Parent = flex;

        stack.Parent = border;

        text.Parent = stack;
        button.Parent = stack;
        button2.Parent = stack;
        button3.Parent = stack;
        button4.Parent = stack;
        textField.Parent = stack;
        flex.Parent = stack;
        image.Parent = stack;

        //border.Parent = uiManager.Root;

        for (int x = 0; x < 256; x++)
        {
            for (int y = 0; y < 256; y++)
            {
                AssetHandle<Scene> scene2 = _assetService.FromPath<Scene>("/Content/FooScene2.xml");
                var newEntity = _sceneSystem.Instantiate(scene2);
                newEntity.Get<Transform>().Position = new Vector3(x, y, 0);
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        Vector2 mov = Vector2.Zero;
        if (_inputService.Keyboard.IsDown(Key.W))
        {
            mov.Y = 1f;
        }
        if (_inputService.Keyboard.IsDown(Key.S))
        {
            mov.Y = -1f;
        }
        if (_inputService.Keyboard.IsDown(Key.A))
        {
            mov.X = -1f;
        }
        if (_inputService.Keyboard.IsDown(Key.D))
        {
            mov.X = 1f;
        }

        if (_inputService.Mouse.IsDown(MouseButton.Right))
        {
            AssetHandle<Scene> scene = _assetService.FromPath<Scene>("/Content/FooScene2.xml");
            var newEntity = _sceneSystem.Instantiate(scene);
            newEntity.Get<Transform>().Position = _cameraSystem.ScreenToWorld(ref entity.Get<OrthographicCamera>(), _inputService.Mouse.Position).ToVector3();
        }

        entity.Get<OrthographicCamera>().ViewSize += _inputService.Mouse.ScrollDelta * deltaTime;

        entity.Get<Transform>().Position += mov.ToVector3();
    }

    public void OnCameraDraw(CameraDrawContext context)
    {
        _renderAPI.Submit(ctx =>
        {
            _render2D.Begin(ctx);
            _render2D.DrawTexture2D(_textureResource, new Vector2(-10f, -3f), new Vector2(15f, 15f), 0f);
            _render2D.End();
        });
    }

    public override void OnDraw(float deltaTime)
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
