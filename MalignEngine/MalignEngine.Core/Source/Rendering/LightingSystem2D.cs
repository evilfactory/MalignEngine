using Arch.Core;
using Silk.NET.SDL;
using System.Numerics;
using Arch.Core.Extensions;

namespace MalignEngine
{
    public class LightingSystem2D : EntitySystem
    {
        [Dependency]
        protected IRenderingService IRenderingService = default!;

        [Dependency]
        protected PhysicsSystem2D PhysicsSystem2D = default!;

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

        public void DrawLights()
        {
            IRenderingService.Begin(blendingMode: BlendingMode.Additive);
            var query = new QueryDescription().WithAll<Light2D, WorldTransform>();
            EntityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref Light2D light) =>
            {

                if (light.ShadowCasting)
                {
                    int rayCount = 128;
                    float angle = 0;
                    float angleIncrease = 360f / rayCount; // fov / rayCount
                    float viewDistance = transform.Scale.X;

                    List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

                    Vector2? lastPos = null;
                    for (int i = 0; i <= rayCount; i++)
                    {
                        Vector2 pos = transform.Position.ToVector2() + GetVectorFromAngle(angle) * viewDistance;

                        Vector2? hitPoint = null;
                        Vector2 hitNormal = Vector2.Zero;
                        PhysicsSystem2D.RayCast((FixtureData2D fixture, Vector2 point, Vector2 normal, float fraction) =>
                        {
                            hitPoint = point;
                            hitNormal = normal;
                            return fraction;
                        }, transform.Position.ToVector2(), pos);

                        if (hitPoint != null)
                        {
                            pos = hitPoint.Value;
                        }

                        if (lastPos != null)
                        {
                            vertices.Add(new VertexPositionColorTexture
                            {
                                Color = light.Color,
                                Position = transform.Position,
                                TextureCoordinate = new Vector2(0.5f, 0.5f)
                            });

                            vertices.Add(new VertexPositionColorTexture
                            {
                                Color = light.Color,
                                Position = transform.Position,
                                TextureCoordinate = new Vector2(0.5f, 0.5f)
                            });

                            vertices.Add(new VertexPositionColorTexture
                            {
                                Color = light.Color,
                                Position = new Vector3(lastPos.Value, 0f),
                                TextureCoordinate = new Vector2(0.5f, 0.5f)
                            });

                            vertices.Add(new VertexPositionColorTexture
                            {
                                Color = light.Color,
                                Position = new Vector3(pos, 0f),
                                TextureCoordinate = new Vector2(0.5f, 0.5f)
                            });
                        }

                        lastPos = pos;
                        angle -= angleIncrease;
                    }
                    IRenderingService.DrawVertices(light.Texture, vertices.ToArray());
                }
                else
                {
                    IRenderingService.DrawTexture2D(light.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), light.Color, transform.ZAxis, 0f);
                }
            });
            IRenderingService.End();
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
        protected IRenderingService IRenderingService = default!;
        [Dependency]
        protected LightingSystem2D LightingSystem2D = default!;

        private RenderTexture lightingTexture;
        private Material lightingMaterial;
        private Material blendMaterial;


        public override void OnInitialize()
        {
            using (Stream file = File.OpenRead("Content/Lighting.glsl"))
            {
                Shader shader = IRenderingService.LoadShader(file);
                lightingMaterial = new Material(shader);
            }

            using (Stream file = File.OpenRead("Content/LightingBlend.glsl"))
            {
                Shader shader = IRenderingService.LoadShader(file);
                blendMaterial = new Material(shader);
            }

            lightingTexture = new RenderTexture(800, 600);
        }

        public override void Process(RenderTexture source)
        {
            lightingTexture.Resize(source.Width, source.Height);

            IRenderingService.SetRenderTarget(lightingTexture);
            IRenderingService.Clear(LightingSystem2D.GetAmbientColor());
            LightingSystem2D.DrawLights();

            lightingMaterial.SetProperty("uLightingTexture", source);

            IRenderingService.SetRenderTarget(source);
            IRenderingService.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f), lightingMaterial);
            IRenderingService.DrawTexture2D(lightingTexture, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Color.White, 0f, 0f);
            IRenderingService.End();
        }
    }
}