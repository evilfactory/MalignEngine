using System.Numerics;
using System.Runtime.InteropServices;

namespace MalignEngine.Sample;

public class ManualCube3D : IService, IUpdate, IPostDrawGUI, IStateEnter<GameState>
{
    private IRenderingAPI RenderingAPI;

    [Dependency]
    private EntityManager EntityManager = default!;

    [Dependency]
    protected InputSystem InputSystem = default!;

    private BufferObject<float> vertexBuffer;
    private BufferObject<uint> indexBuffer;
    private VertexArrayObject vertexArray;

    private Shader shader;
    private Texture2D texture;

    private float[] vertices;

    private Vector3 cameraPosition = new Vector3(0, 0, 0);
    private Quaternion cameraRotation = Quaternion.Identity;

    public ManualCube3D(IRenderingAPI renderingAPI)
    {
        RenderingAPI = renderingAPI;

        vertices = new float[] {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // Bottom-left
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, // top-right
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f, // bottom-right         
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, // top-right
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // bottom-left
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, // top-left
            // Front face
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, // bottom-left
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, // bottom-right
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f, // top-right
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f, // top-right
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, // top-left
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, // bottom-left
            // Left face
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // top-right
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, // top-left
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // bottom-left
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // bottom-left
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, // bottom-right
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // top-right
            // Right face
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // top-left
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // bottom-right
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, // top-right         
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // bottom-right
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // top-left
             0.5f, -0.5f,  0.5f,  0.0f, 0.0f, // bottom-left     
            // Bottom face
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // top-right
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f, // top-left
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, // bottom-left
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, // bottom-left
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, // bottom-right
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, // top-right
            // Top face
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, // top-left
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // bottom-right
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, // top-right     
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, // bottom-right
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, // top-left
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f  // bottom-left    
        };

        vertexBuffer = RenderingAPI.CreateBuffer<float>(vertices, BufferObjectType.Vertex, BufferUsageType.Static);
        //indexBuffer = RenderingAPI.CreateBuffer<uint>(indices, BufferObjectType.Element, BufferUsageType.Static);

        vertexArray = RenderingAPI.CreateVertexArray();
        vertexArray.PushVertexAttribute(3, VertexAttributeType.Float);
        vertexArray.PushVertexAttribute(2, VertexAttributeType.Float);

        using (Stream stream = File.OpenRead("Content/ManualCube3D.glsl"))
        {
            shader = RenderingAPI.CreateShader(stream);
        }

        texture = new Texture2D().LoadFromPath("Content/Textures/mozart.jpg");
    }

    public void OnStateEnter(GameState state)
    {
        if (state != GameState.ManualCube3D) { return; }

        EntityRef camera = EntityManager.World.CreateEntity();
        camera.Add(new Transform());
        camera.Add(new OrthographicCamera() { ClearColor = Color.SkyBlue, IsMain = true, ViewSize = 10 });

        EntityRef testEntity = EntityManager.World.CreateEntity();
        testEntity.Add(new Transform() { Position = new Vector3(0, 0, 0), Scale = new Vector3(1, 1, 1) });
        testEntity.Add(new SpriteRenderer() { Sprite = new Sprite(Texture2D.White), Color = Color.Red });
    }

    public void OnUpdate(float deltaTime)
    {
        if (InputSystem.IsKeyDown(Key.W))
        {
            cameraPosition += Vector3.Transform(-Vector3.UnitZ, cameraRotation);
        }
        if (InputSystem.IsKeyDown(Key.S))
        {
            cameraPosition -= Vector3.Transform(-Vector3.UnitZ, cameraRotation);
        }
        if (InputSystem.IsKeyDown(Key.A))
        {
            cameraPosition.X += 0.1f;
        }
        if (InputSystem.IsKeyDown(Key.D))
        {
            cameraPosition.X -= 0.1f;
        }
        if (InputSystem.IsKeyDown(Key.Space))
        {
            cameraPosition.Y += 0.1f;
        }
        if (InputSystem.IsKeyDown(Key.LeftShift))
        {
            cameraPosition.Y -= 0.1f;
        }

        Vector2 delta = -InputSystem.MouseDelta;

        cameraRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, delta.X * deltaTime * 0.1f);
        cameraRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, delta.Y * deltaTime * 0.1f);
    }

    private float rotation = 0;

    public void OnPostDrawGUI(float deltaTime)
    {
        rotation += deltaTime;

        RenderingAPI.SetTexture(texture, 0);

        float xCam = MathF.Sin(rotation) * 2;
        float yCam = MathF.Cos(rotation) * 2;
        RenderingAPI.Clear(Color.AliceBlue);

        shader.Set("texture1", 0);
        shader.Set("uProjection", Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 16f / 9f, 0.1f, 100f));
        shader.Set("uView", Matrix4x4.CreateLookAt(cameraPosition, cameraPosition + Vector3.Transform(-Vector3.UnitZ, cameraRotation), Vector3.UnitY));

        RenderingAPI.SetBlendingMode(BlendingMode.AlphaBlend);
        RenderingAPI.SetShader(shader);

        for (int x = -15; x < 15; x++)
        {
            for (int y = -15; y < 15; y++)
            {
                for (int z = -15; z < 15; z++)
                {
                    Matrix4x4 model = Matrix4x4.CreateTranslation(new Vector3(x * 2, y * 2, z * 2));
                    shader.Set("uModel", model);
                    RenderingAPI.DrawArrays<float>(vertexBuffer, vertexArray, 36);
                }
            }
        }
    }
}
