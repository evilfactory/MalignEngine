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

            RenderingSystem.Begin(Matrix4x4.Identity, null, blendingMode: BlendingMode.Additive);
            query = new QueryDescription().WithAll<GlobalLight2D, WorldTransform>();
            World.Query(query, (Entity entity, ref WorldTransform pos, ref GlobalLight2D light) =>
            {
                RenderingSystem.DrawTexture2D(Texture2D.White, pos.Position.ToVector2(), new Vector2(100f, 100f), new Vector2(0.5f, 0.5f), new Rectangle(), light.Color, 0, 0f);
            });
            RenderingSystem.End();
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
            RenderingSystem.Clear(Color.Black);
            LightingSystem2D.DrawLights();

            lightingMaterial.SetProperty("uLightingTexture", source);

            RenderingSystem.SetRenderTarget(source);
            RenderingSystem.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, source.Width, source.Height, 0, 0.001f, 100f), lightingMaterial);
            RenderingSystem.DrawRenderTexture(lightingTexture, new Vector2(source.Width / 2f, source.Height / 2f), new Vector2(source.Width, source.Height), Vector2.Zero, new Rectangle(0, 0, 800, 600), Color.White, 0f, 10f);
            RenderingSystem.End();
        }
    }
}