using Arch.Core;
using Silk.NET.SDL;
using System.Numerics;
using Arch.Core.Extensions;

namespace MalignEngine
{
    public class LightingSystem2D : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        public void DrawLights()
        {
            RenderingSystem.Begin(blendingMode: BlendingMode.Additive);
            var query = new QueryDescription().WithAll<Light2D, WorldTransform>();
            World.Query(query, (Entity entity, ref WorldTransform transform, ref Light2D light) =>
            {
                RenderingSystem.DrawTexture2D(light.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), new Vector2(0.5f, 0.5f), new Rectangle(), light.Color, transform.ZAxis, 0f);
            });
            RenderingSystem.End();
        }

        public Color GetAmbientColor()
        {
            Color color = Color.Black;
            var query = new QueryDescription().WithAll<GlobalLight2D>();
            World.Query(query, (Entity entity, ref GlobalLight2D light) =>
            {
                color = light.Color;
            });

            return color;
        }
    }

    public class LightingPostProcessingSystem2D : PostProcessBaseSystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;
        [Dependency]
        protected LightingSystem2D LightingSystem2D = default!;

        private RenderTexture lightingTexture;
        private Material lightingMaterial;
        private Material blendMaterial;


        public override void OnInitialize()
        {
            using (Stream file = File.OpenRead("Content/Lighting.glsl"))
            {
                Shader shader = RenderingSystem.LoadShader(file);
                lightingMaterial = new Material(shader);
            }

            using (Stream file = File.OpenRead("Content/LightingBlend.glsl"))
            {
                Shader shader = RenderingSystem.LoadShader(file);
                blendMaterial = new Material(shader);
            }

            lightingTexture = new RenderTexture(800, 600);
        }

        public override void Process(RenderTexture source)
        {
            lightingTexture.Resize(source.Width, source.Height);

            RenderingSystem.SetRenderTarget(lightingTexture);
            RenderingSystem.Clear(LightingSystem2D.GetAmbientColor());
            LightingSystem2D.DrawLights();

            lightingMaterial.SetProperty("uLightingTexture", source);

            RenderingSystem.SetRenderTarget(source);
            RenderingSystem.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.001f, 100f), lightingMaterial);
            RenderingSystem.DrawRenderTexture(lightingTexture, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), Vector2.Zero, new Rectangle(0, 0, 800, 600), Color.White, 0f, 0f);
            RenderingSystem.End();
        }
    }
}