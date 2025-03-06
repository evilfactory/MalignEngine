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
    public class GLRenderingSystem : BaseRenderingService
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

        protected ILogger Logger;

        internal GL openGL;

        private const uint MaxBatchCount = 5000;
        private const uint MaxIndexCount = MaxBatchCount * 6;
        private const uint MaxTextures = 16;

        private BufferObject<Vertex> vbo;
        private BufferObject<uint> ebo;
        private VertexArrayObject<Vertex, uint> vao;
        private GLTextureHandle[] textures;

        private uint indexCount = 0;
        private uint batchIndex = 0;

        private Vertex[] vertices;
        private uint[] triangleIndices;

        private bool drawing = false;
        private Material drawingMaterial;
        private Matrix4x4 drawingMatrix;

        private Matrix4x4 currentMatrix;
        private Material basicMaterial;
        private RenderTexture renderTexture;

        private Shader postProcessingShader;

        private uint vertexSize = (uint)Marshal.SizeOf<Vertex>();

        public override void OnInitialize()
        {
            Logger = LoggerService.GetSawmill("rendering");

            openGL = GL.GetApi(Window.window);

            openGL.Enable(GLEnum.Blend);
            openGL.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            openGL.Enable(GLEnum.DepthTest);
            openGL.Enable(GLEnum.StencilTest);

            openGL.DepthFunc(GLEnum.Lequal);

            openGL.Enable(GLEnum.DebugOutput);
            openGL.Enable(GLEnum.DebugOutputSynchronous);
            unsafe
            {
                openGL.DebugMessageCallback(GLDebugMessageCallback, null);
            }

            textures = new GLTextureHandle[MaxTextures];

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

            vertices = new Vertex[MaxBatchCount * 4];

            ebo = new BufferObject<uint>(openGL, triangleIndices, BufferTargetARB.ElementArrayBuffer);
            vbo = new BufferObject<Vertex>(openGL, vertices, BufferTargetARB.ArrayBuffer);
            vao = new VertexArrayObject<Vertex, uint>(openGL, vbo, ebo);

            // vertex data layout
            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, vertexSize, 0 * 4); // position
            vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, vertexSize, 3 * 4); // uv
            vao.VertexAttributePointer(2, 1, VertexAttribPointerType.Float, vertexSize, 5 * 4); // texture index
            vao.VertexAttributePointer(3, 4, VertexAttribPointerType.UnsignedByte, vertexSize, 6 * 4); // color

            currentMatrix = Matrix4x4.Identity;

            using (Stream file = File.OpenRead("Content/SpriteShader.glsl"))
            {
                basicMaterial = new Material((GLShader)LoadShader(file));
                basicMaterial.UseTextureBatching = true;
            }

            StringBuilder extensions = new StringBuilder();
            int numExtensions = openGL.GetInteger(GLEnum.NumExtensions);
            for (int i = 0; i < numExtensions; i++)
            {
                extensions.Append(openGL.GetStringS(GLEnum.Extensions, (uint)i));
                extensions.Append(" ");
            }

            Logger.LogInfo($"Initialized OpenGL renderer. \n OpenGL {openGL.GetStringS(GLEnum.Version)}\n Shading Language {openGL.GetStringS(GLEnum.ShadingLanguageVersion)} \n GPU: {openGL.GetStringS(GLEnum.Renderer)} \n Vendor: {openGL.GetStringS(GLEnum.Vendor)}");
        }

        private void GLDebugMessageCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
        {
            string stringMessage = SilkMarshal.PtrToString(message);

            Logger.LogError($"{id}: {type} of {severity}, raised from {source}: {stringMessage}\n{Environment.StackTrace}");

            Debug.Assert(false);
        }

        public override void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend)
        {
            drawing = true;
            drawingMaterial = material ?? basicMaterial;
            drawingMatrix = matrix;
            textures = new GLTextureHandle[MaxTextures];

            batchIndex = 0;

            switch (blendingMode)
            {
                case BlendingMode.AlphaBlend:
                    openGL.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
                    break;
                case BlendingMode.Additive:
                    openGL.BlendFunc(GLEnum.One, GLEnum.One);
                    break;
            }
        }

        public override void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend)
        {
            Begin(currentMatrix, material, blendingMode);
        }

        public override void Clear(Color color)
        {
            openGL.ClearColor(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            openGL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        public override void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth)
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

        public override void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, Color color, float rotation, float layerDepth)
        {
            DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, color, rotation, layerDepth);
        }

        public override void DrawTexture2D(ITexture texture, Vector2 position, Vector2 size, float layerDepth)
        {
            DrawTexture2D(texture, position, size, Vector2.Zero, Vector2.One, Color.White, 0f, layerDepth);
        }

        public override void DrawQuad(ITexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft)
        {
            DrawVertices((GLTextureHandle)texture.Handle, new VertexPositionColorTexture[] { topRight, bottomRight, bottomLeft, topLeft });
        }

        public override void DrawVertices(ITexture texture, VertexPositionColorTexture[] vertices)
        {
            DrawVertices((GLTextureHandle)texture.Handle, vertices);
        }

        private void DrawVertices(GLTextureHandle texture, VertexPositionColorTexture[] vertices)
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
                this.vertices[batchIndex] = new Vertex(vertices[i].Position, vertices[i].TextureCoordinate, textureSlot, vertices[i].Color);
                batchIndex++;
            }

            uint amountQuads = (uint)vertices.Length / 4;

            indexCount += amountQuads * 6;
        }

        public override void End()
        {
            vbo.Bind();
            vao.Bind();
            ebo.Bind();

            unsafe
            {
                fixed (void* verticePtr = vertices)
                {
                    openGL.BufferSubData(GLEnum.ArrayBuffer, 0, batchIndex * vertexSize, verticePtr);
                }
            }

            GLShader drawingShader = (GLShader)drawingMaterial.Shader;

            drawingShader.Use();

            drawingShader.SetUniform("uModel", Matrix4x4.Identity);
            drawingShader.SetUniform("uView", Matrix4x4.Identity);
            drawingShader.SetUniform("uProjection", drawingMatrix);

            drawingShader.SetUniform("uTextures", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            // First 16 textures are reserved for the texture batching
            int textureIndex = (int)MaxTextures;
            foreach (var property in drawingMaterial.GetProperties())
            {
                object propertyValue = property.Value;

                if (propertyValue is int)
                {
                    drawingShader.SetUniform(property.Key, (int)propertyValue);
                }
                else if (propertyValue is float)
                {
                    drawingShader.SetUniform(property.Key, (float)propertyValue);
                }
                else if (propertyValue is Color)
                {
                    drawingShader.SetUniform(property.Key, (Color)propertyValue);
                }
                else if (propertyValue is Matrix4x4)
                {
                    drawingShader.SetUniform(property.Key, (Matrix4x4)propertyValue);
                }
                else if (propertyValue is ITexture)
                {
                    ((GLTextureHandle)((ITexture)propertyValue).Handle).Bind(TextureUnit.Texture0 + (int)textureIndex);
                    drawingShader.SetUniform(property.Key, textureIndex);
                    textureIndex++;
                }
            }

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null) { break; }

                TextureUnit textureUnit = TextureUnit.Texture0 + i;
                textures[i].Bind(textureUnit);
            }

            unsafe
            {
                openGL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, null);
            }

            indexCount = 0;
            drawing = false;
        }

        public override TextureHandle CreateTextureHandle()
        {
            return new GLTextureHandle(openGL);
        }

        public override Shader LoadShader(Stream data)
        {
            StreamReader reader = new StreamReader(data);
            return new GLShader(openGL, reader.ReadToEnd());
        }

        public override void SetMatrix(Matrix4x4 matrix)
        {
            currentMatrix = matrix;
        }

        public override void SetRenderTarget(RenderTexture renderTexture, int width = 0, int height = 0)
        {
            this.renderTexture = renderTexture;

            if (this.renderTexture != null)
            {
                if (width == 0) { width = renderTexture.Width; }
                if (height == 0) { height = renderTexture.Height; }
                ((GLTextureHandle)this.renderTexture.Handle).BindAsRenderTarget();
                openGL.Viewport(0, 0, (uint)width, (uint)height);
            }
            else
            {
                openGL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                openGL.Viewport(0, 0, (uint)Window.Width, (uint)Window.Height);
            }
        }

        public override void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass)
        {
            GLEnum functiongl = GLEnum.Always;

            switch (function)
            {
                case StencilFunction.Equal:
                    functiongl = GLEnum.Equal;
                    break;
                case StencilFunction.NotEqual:
                    functiongl = GLEnum.Notequal;
                    break;
                case StencilFunction.Less:
                    functiongl = GLEnum.Less;
                    break;
                case StencilFunction.LessThanOrEqual:
                    functiongl = GLEnum.Lequal;
                    break;
                case StencilFunction.Greater:
                    functiongl = GLEnum.Greater;
                    break;
                case StencilFunction.GreaterThanOrEqual:
                    functiongl = GLEnum.Gequal;
                    break;
                case StencilFunction.Always:
                    functiongl = GLEnum.Always;
                    break;
                case StencilFunction.Never:
                    functiongl = GLEnum.Never;
                    break;
            }

            GLEnum OperationToGl(StencilOperation operation)
            {
                switch (operation)
                {
                    case StencilOperation.Keep:
                        return GLEnum.Keep;
                    case StencilOperation.Zero:
                        return GLEnum.Zero;
                    case StencilOperation.Replace:
                        return GLEnum.Replace;
                    case StencilOperation.Increment:
                        return GLEnum.Incr;
                    case StencilOperation.IncrementWrap:
                        return GLEnum.IncrWrap;
                    case StencilOperation.Decrement:
                        return GLEnum.Decr;
                    case StencilOperation.DecrementWrap:
                        return GLEnum.DecrWrap;
                    case StencilOperation.Invert:
                        return GLEnum.Invert;
                    default:
                        return GLEnum.Keep;
                }
            }

            openGL.StencilOp(OperationToGl(fail), OperationToGl(zfail), OperationToGl(zpass));
            openGL.StencilFunc(functiongl, reference, mask);
        }
    }
}