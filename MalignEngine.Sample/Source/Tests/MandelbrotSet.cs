using System.Numerics;
using Silk.NET.SDL;

namespace MalignEngine.Sample;

public class MandelbrotSet : BaseSystem, IDrawGUI
{
    [Dependency] 
    protected WindowService WindowService = default!;
    [Dependency]
    protected IRenderingAPI RenderingAPI = default!;
    [Dependency]
    protected IRenderer2D Renderer2D = default!;
    [Dependency] 
    protected InputSystem InputSystem = default!;
    
    private Shader mandelbrotShader;
    private IBufferObject<Renderer2D.Vertex> vertexBuffer;
    private VertexArrayDescriptor vertexArrayObject;

    private float scale = 1f;
    private float movex = 0f;
    private float movey = 0f;
    
    public override void OnInitialize()
    {
        using (FileStream stream = File.OpenRead("Content/MandelbrotSet.glsl"))
        {
            mandelbrotShader= RenderingAPI.CreateShader((stream));
        }

        Renderer2D.Vertex[] vertices = new[]
        {
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(0, 0, 1), TextureIndex = 0f },
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(1, 0, 1), TextureIndex = 0f },
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(0, 1, 1), TextureIndex = 0f },
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(1, 1, 1), TextureIndex = 0f },
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(0, 1, 1), TextureIndex = 0f },
            new Renderer2D.Vertex(){ Color = Color.White, Position = new Vector3(1, 0, 1), TextureIndex = 0f },
        };

        vertexBuffer = RenderingAPI.CreateBuffer<Renderer2D.Vertex>(vertices, BufferObjectType.Vertex, BufferUsageType.Static);
        vertexArrayObject = RenderingAPI.CreateVertexArray();
        
    }

    public override void OnUpdate(float deltaTime)
    {
        if (InputSystem.IsKeyDown(Key.D))
        {
            movex += deltaTime / scale;
        }

        if (InputSystem.IsKeyDown(Key.A))
        {
            movex -= deltaTime / scale;
        }

        if (InputSystem.IsKeyDown(Key.W))
        {
            movey += deltaTime / scale;
        }

        if (InputSystem.IsKeyDown(Key.S))
        {
            movey -= deltaTime / scale;
        }
        
        scale = scale + InputSystem.MouseScroll * deltaTime * scale;
    }

    public void OnDrawGUI(float deltaTime)
    {
        mandelbrotShader.Set("u_iterations", 500);
        mandelbrotShader.Set("u_movex", movex);
        mandelbrotShader.Set("u_movey", movey);
        mandelbrotShader.Set("u_scale", scale);
        Renderer2D.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 1f, 0f, 0.001f, 100f), new Material(mandelbrotShader));
        Renderer2D.DrawTexture2D(Texture2D.White, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), 0f);
        Renderer2D.End();

        /*
        RenderingAPI.SetTexture(Texture2D.White, 0);
        RenderingAPI.SetShader(mandelbrotShader);
        mandelbrotShader.Set("uModel", Matrix4x4.Identity);
        mandelbrotShader.Set("uView", Matrix4x4.Identity);
        mandelbrotShader.Set("uProjection", Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 1f, 0f, 0.001f, 100f));

        mandelbrotShader.Set("uTextures", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

        //mandelbrotShader.Set("u_iterations", 5);

        RenderingAPI.DrawArrays(vertexBuffer, vertexArrayObject, 6, PrimitiveType.Triangles);
        */
    }
}