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

        public void DrawLights()
        {
            IRenderingService.Begin(blendingMode: BlendingMode.Additive);
            var query = new QueryDescription().WithAll<Light2D, WorldTransform>();
            EntityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref Light2D light) =>
            {
                IRenderingService.DrawTexture2D(light.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), light.Color, transform.ZAxis, 0f);
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