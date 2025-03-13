using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using Silk.NET.Maths;

namespace MalignEngine
{
    public struct VertexPositionColorTexture
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
    }

    public interface IRenderer2D : IService
    {
        public bool FlipY { get; set; }
        public void Clear(Color color);
        public void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public void End();
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth);
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Color color, float rotation, float layerDepth);
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, float layerDepth);
        public void DrawQuad(ITexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
        public void SetMatrix(Matrix4x4 matrix);
    }

    public class Renderer2D : IRenderer2D, IInit
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector3 Position;
            public Vector2 UV;
            public float TextureIndex;
            public Color Color;

            public Vertex(Vector3 positon, Vector2 uv, float textureIndex, Color color)
            {
                Position = positon;
                UV = uv;
                TextureIndex = textureIndex;
                Color = color;
            }
        }

        [Dependency]
        protected WindowService Window = default!;
        [Dependency]
        protected LoggerService LoggerService = default!;
        [Dependency]
        protected IRenderingAPI RenderingAPI = default!;

        protected ILogger Logger;

        private const uint MaxBatchCount = 5000;
        private const uint MaxIndexCount = MaxBatchCount * 6;
        private const uint MaxTextures = 16;

        private BufferObject<Vertex> vbo;
        private BufferObject<uint> ebo;
        private VertexArrayObject vao;
        private ITexture[] textures;

        private uint indexCount = 0;
        private uint batchIndex = 0;

        private Vertex[] batchVertices;
        private uint[] triangleIndices;

        private bool drawing = false;
        private Material drawingMaterial;
        private Matrix4x4 drawingMatrix;

        private Matrix4x4 currentMatrix;
        private Material basicMaterial;

        public bool FlipY { get; set; }

        public void OnInitialize()
        {
            Logger = LoggerService.GetSawmill("rendering.2d");

            textures = new ITexture[MaxTextures];

            triangleIndices = new uint[MaxIndexCount];
            uint offset = 0;

            for (uint i = 0; i < MaxIndexCount; i += 6)
            {
                triangleIndices[i + 0] = 0 + offset;
                triangleIndices[i + 1] = 1 + offset;
                triangleIndices[i + 2] = 3 + offset;

                triangleIndices[i + 3] = 1 + offset;
                triangleIndices[i + 4] = 2 + offset;
                triangleIndices[i + 5] = 3 + offset;

                offset += 4;
            }

            batchVertices = new Vertex[MaxBatchCount * 4];

            ebo = RenderingAPI.CreateBuffer<uint>(triangleIndices, BufferObjectType.Element, BufferUsageType.Static);
            vbo = RenderingAPI.CreateBuffer<Vertex>(batchVertices, BufferObjectType.Vertex, BufferUsageType.Static);
            vao = RenderingAPI.CreateVertexArray();

            // vertex data layout
            vao.PushVertexAttribute(3, VertexAttributeType.Float); // position
            vao.PushVertexAttribute(2, VertexAttributeType.Float); // uv
            vao.PushVertexAttribute(1, VertexAttributeType.Float); // texture index
            vao.PushVertexAttribute(4, VertexAttributeType.UnsignedByte); // color

            currentMatrix = Matrix4x4.Identity;

            using (Stream file = File.OpenRead("Content/SpriteShader.glsl"))
            {
                basicMaterial = new Material(RenderingAPI.CreateShader(file));
                basicMaterial.UseTextureBatching = true;
            }
        }

        public void SetMatrix(Matrix4x4 matrix)
        {
            currentMatrix = matrix;
        }

        public void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend)
        {
            drawing = true;
            drawingMaterial = material ?? basicMaterial;
            drawingMatrix = matrix;
            textures = new ITexture[MaxTextures];

            batchIndex = 0;
        }

        public void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend)
        {
            Begin(currentMatrix, material, blendingMode);
        }

        public void Clear(Color color)
        {
            RenderingAPI.Clear(color);
        }

        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth)
        {
            Vector3 topRightPos = new Vector3(0.0f + size.X / 2f, 0.0f + size.Y / 2f, 0f);
            topRightPos = Vector3.Transform(topRightPos, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation));
            topRightPos = Vector3.Transform(topRightPos, Matrix4x4.CreateTranslation(new Vector3(position, layerDepth)));

            Vector3 bottomRightPos = new Vector3(0.0f + size.X / 2f, 0.0f - size.Y / 2f, 0f);
            bottomRightPos = Vector3.Transform(bottomRightPos, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation));
            bottomRightPos = Vector3.Transform(bottomRightPos, Matrix4x4.CreateTranslation(new Vector3(position, layerDepth)));

            Vector3 bottomLeftPos = new Vector3(0.0f - size.X / 2f, 0.0f - size.Y / 2f, 0f);
            bottomLeftPos = Vector3.Transform(bottomLeftPos, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation));
            bottomLeftPos = Vector3.Transform(bottomLeftPos, Matrix4x4.CreateTranslation(new Vector3(position, layerDepth)));

            Vector3 topLeftPos = new Vector3(0.0f - size.X / 2f, 0.0f + size.Y / 2f, 0f);
            topLeftPos = Vector3.Transform(topLeftPos, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotation));
            topLeftPos = Vector3.Transform(topLeftPos, Matrix4x4.CreateTranslation(new Vector3(position, layerDepth)));

            DrawQuad(texture,
                new VertexPositionColorTexture(topRightPos, color, new Vector2(uv2.X, uv2.Y)), // top right 1f, 1f
                new VertexPositionColorTexture(bottomRightPos, color, new Vector2(uv2.X, uv1.Y)), // bottom right 1f, 0f
                new VertexPositionColorTexture(bottomLeftPos, color, new Vector2(uv1.X, uv1.Y)), // bottom left 0f, 0f
                new VertexPositionColorTexture(topLeftPos, color, new Vector2(uv1.X, uv2.Y)) // top left 0f, 1f
            );
        }

        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, Color color, float rotation, float layerDepth)
        {
            DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, color, rotation, layerDepth);
        }

        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, float layerDepth)
        {
            DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, Color.White, 0f, layerDepth);
        }

        public void DrawQuad(ITexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft)
        {
            DrawVertices(texture, new VertexPositionColorTexture[] { topRight, bottomRight, bottomLeft, topLeft });
        }

        public void DrawVertices(ITexture texture, VertexPositionColorTexture[] vertices)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Begin must be called before Draw.");
            }

            if (texture == null) { throw new ArgumentNullException(nameof(texture)); }

            // Don't batch draw multiple textures if texture batching is disabled
            if (indexCount >= MaxIndexCount || (!drawingMaterial.UseTextureBatching && texture != textures[0]))
            {
                End();
                Begin(drawingMatrix, drawingMaterial);
            }

            int textureSlot = -1;

            for (int i = 0; i < textures.Length; i++)
            {
                if (texture == null) { break; }

                if (textures[i] == null)
                {
                    textures[i] = texture;
                    textureSlot = i;
                    break;
                }

                if (textures[i] == texture)
                {
                    textureSlot = i;
                    break;
                }
            }

            if (FlipY)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].TextureCoordinate.Y = 1f - vertices[i].TextureCoordinate.Y;
                }
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                batchVertices[batchIndex] = new Vertex(vertices[i].Position, vertices[i].TextureCoordinate, textureSlot, vertices[i].Color);
                batchIndex++;
            }

            uint amountQuads = (uint)vertices.Length / 4;

            indexCount += amountQuads * 6;
        }

        public void End()
        {
            vbo.BufferData(batchVertices, 0, batchIndex);

            Shader drawingShader = drawingMaterial.Shader;

            RenderingAPI.SetShader(drawingShader);

            drawingShader.Set("uModel", Matrix4x4.Identity);
            drawingShader.Set("uView", Matrix4x4.Identity);
            drawingShader.Set("uProjection", drawingMatrix);

            drawingShader.Set("uTextures", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            // First 16 textures are reserved for the texture batching
            int textureIndex = (int)MaxTextures;
            foreach (var property in drawingMaterial.GetProperties())
            {
                object propertyValue = property.Value;

                if (propertyValue is int)
                {
                    drawingShader.Set(property.Key, (int)propertyValue);
                }
                else if (propertyValue is float)
                {
                    drawingShader.Set(property.Key, (float)propertyValue);
                }
                else if (propertyValue is Color)
                {
                    drawingShader.Set(property.Key, (Color)propertyValue);
                }
                else if (propertyValue is Matrix4x4)
                {
                    drawingShader.Set(property.Key, (Matrix4x4)propertyValue);
                }
                else if (propertyValue is ITexture)
                {
                    RenderingAPI.SetTexture((ITexture)propertyValue, textureIndex);
                    drawingShader.Set(property.Key, textureIndex);
                    textureIndex++;
                }
            }

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null) { break; }

                RenderingAPI.SetTexture(textures[i], i);
            }

            RenderingAPI.DrawIndexed(ebo, vbo, vao, indexCount);

            indexCount = 0;
            drawing = false;
        }
    }
}