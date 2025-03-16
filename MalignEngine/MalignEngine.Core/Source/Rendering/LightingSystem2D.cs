using Arch.Core;
using Silk.NET.SDL;
using System.Numerics;
using Arch.Core.Extensions;

namespace MalignEngine
{
    public class LightingSystem2D : EntitySystem
    {
        [Dependency]
        protected IRenderingAPI RenderingAPI = default!;
        [Dependency]
        protected IRenderer2D Renderer2D = default!;
        [Dependency]
        protected PhysicsSystem2D PhysicsSystem2D = default!;
        [Dependency]
        protected CameraSystem CameraSystem = default!;
        [Dependency]
        protected TransformSystem TransformSystem = default!;


        private Shader shader;
        private VertexArrayObject vao;
        private BufferObject<Renderer2D.Vertex> vbo;

        private static Vector2 GetVectorFromAngle(float angle)
        {
            float angleRad = angle * (MathF.PI / 180f);
            return new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad));
        }

        private static float GetAngleFromVector(Vector2 vector)
        {
            vector = Vector2.Normalize(vector);
            float n = MathF.Atan2(vector.Y, vector.X) * (180f / MathF.PI);
            if (n < 0) { n += 360; }

            return n;
        }

        private static Vector2 NormalizeToSquare(Vector2 vector)
        {
            float maxComponent = Math.Max(Math.Abs(vector.X), Math.Abs(vector.Y));

            if (maxComponent == 0)
            {
                return Vector2.Zero; // Avoid division by zero
            }

            return vector / maxComponent;
        }


        public override void OnInitialize()
        {
            EventService.Get<ComponentEventChannel<ComponentInitEvent>>().Subscribe<Light2D>(InitLight2D);
            EventService.Get<ComponentEventChannel<TransformMoveEvent>>().Subscribe<Light2D>(UpdateLight2D);

            vbo = RenderingAPI.CreateBuffer(new Span<Renderer2D.Vertex>(), BufferObjectType.Vertex, BufferUsageType.Static);
            vao = RenderingAPI.CreateVertexArray();

            // vertex data layout
            vao.PushVertexAttribute(3, VertexAttributeType.Float); // position
            vao.PushVertexAttribute(2, VertexAttributeType.Float); // uv
            vao.PushVertexAttribute(1, VertexAttributeType.Float); // texture index
            vao.PushVertexAttribute(4, VertexAttributeType.UnsignedByte); // color

            using (Stream file = File.OpenRead("Content/SpriteShader.glsl"))
            {
                shader = RenderingAPI.CreateShader(file);
            }
        }

        private void UpdateLight2D(EntityRef entity, TransformMoveEvent eventArgs)
        {
            ref Light2D light = ref entity.Get<Light2D>();
            light.ShadowGeometryDirty = true;
        }

        private void InitLight2D(EntityRef entity, ComponentInitEvent eventData)
        {
            ref Light2D light = ref entity.Get<Light2D>();
            light.ShadowGeometryDirty = true;
        }

        public void ComputeLight(EntityRef entity)
        {
            WorldTransform transform = entity.Get<WorldTransform>();
            ref Light2D light = ref entity.Get<Light2D>();

            float viewDistance = transform.Scale.X;

            List<Vector2> edgesToCheck = new List<Vector2>();

            List<Renderer2D.Vertex> vertices = new List<Renderer2D.Vertex>();

            Vector2 position = transform.Position.ToVector2();

            // start edges with the 4 corners of the light
            edgesToCheck.Add(position + new Vector2(-viewDistance, -viewDistance));
            edgesToCheck.Add(position + new Vector2(-viewDistance, viewDistance));
            edgesToCheck.Add(position + new Vector2(viewDistance, viewDistance));
            edgesToCheck.Add(position + new Vector2(viewDistance, -viewDistance));

            PhysicsSystem2D.QueryAABB((EntityRef entity) =>
            {
                WorldTransform queryTransform = entity.Get<WorldTransform>();
                PhysicsBody2D body = entity.Get<PhysicsBody2D>();

                foreach (Vector2 vertice in body.Fixtures.Select(x => x.Shape).SelectMany(x => x.Vertices))
                {
                    //edgesToCheck.Add(queryTransform.Position.ToVector2() + vertice);
                    // from the center of the light to the vertice, add two more slightly perpendicular offset points to make sure the light reaches the edge
                    Vector2 edge = queryTransform.Position.ToVector2() + vertice;

                    if (Vector2.Distance(edge, position) > viewDistance) { continue; }

                    Vector2 direction = Vector2.Normalize(edge - position);
                    Vector2 offset = new Vector2(-direction.Y, direction.X) * 0.1f;
                    edgesToCheck.Add(edge + offset);
                    edgesToCheck.Add(edge - offset);
                }

                return true;
            }, position - new Vector2(viewDistance, viewDistance), position + new Vector2(viewDistance, viewDistance));

            edgesToCheck.Sort((a, b) =>
            {
                float angleA = GetAngleFromVector(a - position);
                float angleB = GetAngleFromVector(b - position);
                return angleA.CompareTo(angleB);
            });

            Vector2? lastPoint = null;
            for (int i = 0; i < edgesToCheck.Count + 1; i++)
            {
                Vector2 edge;
                if (i == edgesToCheck.Count)
                {
                    edge = edgesToCheck[0];
                }
                else
                {
                    edge = edgesToCheck[i];
                }

                Renderer2D.DrawTexture2D(Texture2D.White, edge, new Vector2(0.1f, 0.1f), Color.Blue, 0f, 0f);

                Vector2 endPoint = position + NormalizeToSquare(edge - position) * viewDistance;

                Vector2? hitPoint = null;
                Vector2 hitNormal = Vector2.Zero;
                PhysicsSystem2D.RayCast((EntityRef hitEntity, Vector2 point, Vector2 normal, float fraction) =>
                {
                    hitPoint = point;
                    hitNormal = normal;
                    return fraction;
                }, position, endPoint);

                if (hitPoint == null)
                {
                    hitPoint = endPoint;
                }

                Renderer2D.DrawTexture2D(Texture2D.White, hitPoint.Value, new Vector2(0.1f, 0.1f), Color.Red, 0f, 0f);

                if (lastPoint != null)
                {
                    vertices.Add(new Renderer2D.Vertex
                    {
                        Color = light.Color,
                        Position = position.ToVector3(),
                        UV = new Vector2(0.5f, 0.5f)
                    });

                    // measure the sin, cos of the angle between the two points to determine the uv coordinates, considering that 0.5 is the center of the texture,
                    // and prevent the texture from being distorted because of the rays hitting too closer
                    Vector2 uv1 = new Vector2(0.5f + (lastPoint.Value.X - position.X) / viewDistance / 2, 0.5f + (lastPoint.Value.Y - position.Y) / viewDistance / 2);
                    Vector2 uv2 = new Vector2(0.5f + (hitPoint.Value.X - position.X) / viewDistance / 2, 0.5f + (hitPoint.Value.Y - position.Y) / viewDistance / 2);

                    vertices.Add(new Renderer2D.Vertex
                    {
                        Color = light.Color,
                        Position = new Vector3(lastPoint.Value, 0f),
                        UV = uv1
                    });

                    vertices.Add(new Renderer2D.Vertex
                    {
                        Color = light.Color,
                        Position = new Vector3(hitPoint.Value, 0f),
                        UV = uv2
                    });
                }

                lastPoint = hitPoint;
            }

            light.ShadowGeometry = vertices.ToArray();
            light.ShadowGeometryDirty = false;
        }

        public void DrawLights()
        {
            RenderingAPI.SetBlendingMode(BlendingMode.Additive);

            Renderer2D.Begin(blendingMode: BlendingMode.Additive);
            var query = new QueryDescription().WithAll<Light2D, WorldTransform>();
            EntityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref Light2D light) =>
            {

                if (light.ShadowCasting)
                {
                    if (light.ShadowGeometryDirty)
                    {
                        ComputeLight(entity);
                    }

                    vbo.BufferData(light.ShadowGeometry);
                    
                    RenderingAPI.SetTexture(light.Texture, 0);
                    RenderingAPI.SetShader(shader);
                    shader.Set("uTextures", new int[] { 0 });
                    shader.Set("uModel", Matrix4x4.Identity);
                    shader.Set("uView", Matrix4x4.Identity);
                    shader.Set("uProjection", CameraSystem.RenderingCamera.Matrix);

                    RenderingAPI.DrawArrays(vbo, vao, (uint)light.ShadowGeometry.Length, PrimitiveType.Triangles);
                }
                else
                {
                    Renderer2D.DrawTexture2D(light.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), light.Color, transform.ZAxis, 0f);
                }
            });
            Renderer2D.End();

            RenderingAPI.SetBlendingMode(BlendingMode.AlphaBlend);
        }

        public Color GetAmbientColor()
        {
            Color color = Color.Black;
            var query = new QueryDescription().WithAll<GlobalLight2D>();
            EntityManager.World.Query(query, (EntityRef entity, ref GlobalLight2D light) =>
            {
                color = light.Color;
            });

            return color;
        }
    }

    public class LightingPostProcessingSystem2D : PostProcessBaseSystem
    {
        [Dependency]
        protected IRenderer2D IRenderer2D = default!;
        [Dependency]
        protected IRenderingAPI IRenderingAPI = default!;
        [Dependency]
        protected LightingSystem2D LightingSystem2D = default!;

        private RenderTexture lightingTexture;
        private Material lightingMaterial;
        private Material blendMaterial;


        public override void OnInitialize()
        {
            using (Stream file = File.OpenRead("Content/Lighting.glsl"))
            {
                Shader shader = IRenderingAPI.CreateShader(file);
                lightingMaterial = new Material(shader);
            }

            using (Stream file = File.OpenRead("Content/LightingBlend.glsl"))
            {
                Shader shader = IRenderingAPI.CreateShader(file);
                blendMaterial = new Material(shader);
            }

            lightingTexture = new RenderTexture(800, 600);
        }

        public override void Process(RenderTexture source)
        {
            lightingTexture.Resize(source.Width, source.Height);

            IRenderingAPI.SetRenderTarget(lightingTexture);
            IRenderer2D.Clear(LightingSystem2D.GetAmbientColor());
            LightingSystem2D.DrawLights();

            lightingMaterial.SetProperty("uLightingTexture", source);

            IRenderingAPI.SetRenderTarget(source);
            IRenderer2D.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f), lightingMaterial);
            IRenderer2D.DrawTexture2D(lightingTexture, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Color.White, 0f, 0f);
            IRenderer2D.End();
        }
    }
}