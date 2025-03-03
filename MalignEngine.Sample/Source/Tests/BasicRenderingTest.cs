using System.Numerics;

namespace MalignEngine.Sample;

public class BasicRenderingTest : IService, IDrawGUI
{
    [Dependency]
    protected IRenderingService RenderingService = default!;
    [Dependency]
    protected WindowService WindowSystem = default!;

    public void OnDrawGUI(float deltaTime)
    {
        RenderingService.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));

        int horizontalSlices = 50;
        int verticalSlices = 50;

        float horizontalStep = WindowSystem.Width / horizontalSlices;
        float verticalStep = WindowSystem.Height / verticalSlices;

        for (int i = 0; i < horizontalSlices; i++)
        {
            for (int j = 0; j < verticalSlices; j++)
            {
                Vector2 size = new Vector2(horizontalStep - 1, verticalStep - 1);
                RenderingService.DrawTexture2D(Texture2D.White, new Vector2(i * horizontalStep, j * verticalStep) + size / 2f, size, new Color((i * 53) % 255, (j * 5) % 255, 255), 0f, 0f);
            }
        }

        RenderingService.End();
    }
}
