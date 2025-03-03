using CommunityToolkit.HighPerformance;
using Silk.NET.OpenGLES;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace MalignEngine
{
    public class Texture2D : Texture, IFileLoadableAsset<Texture2D>
    {
        public static Texture2D White = new Texture2D(new Color[,] { { Color.White } }, 1, 1);

        private Color[,] textureData;

        public Texture2D()
        {
            Width = 1;
            Height = 1;

            CreateHandle();
        }

        public Texture2D(int width, int height)
        {
            Width = width;
            Height = height;

            CreateHandle();
        }

        public Texture2D(Color[,] data, int width, int height)
        {
            textureData = data;
            Width = width;
            Height = height;

            CreateHandle();

            // flatten the 2D array into a 1D array
            Color[] flattenData = new Color[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    flattenData[y * Width + x] = textureData[x, y];
                }
            }
            Handle?.SubmitData(flattenData);
        }

        private void CreateHandle()
        {
            var rendering = Application.Main.ServiceContainer.GetInstance<IRenderingService>();

            if (rendering == null) { return; }

            Handle = rendering.CreateTextureHandle();
            Handle.Initialize(Width, Height, false);
        }

        public override string ToString()
        {
            return $"Texture2D: ({Width}x{Height})";
        }

        public Texture2D LoadFromPath(AssetPath assetPath)
        {
            using (var img = Image.Load<Rgba32>(assetPath))
            {
                Width = img.Width;
                Height = img.Height;
                Handle?.Resize(Width, Height);

                img.Mutate(x => x.AutoOrient());

                Color[,] textureData = new Color[img.Width, img.Height];

                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var rowSpan = accessor.GetRowSpan(y);

                        for (int i = 0; i < rowSpan.Length; i++)
                        {
                            textureData[i, y] = new Color(rowSpan[i].R, rowSpan[i].G, rowSpan[i].B, rowSpan[i].A);
                        }
                    }
                });

                // flatten the 2D array into a 1D array
                Color[] flattenData = new Color[Width * Height];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        flattenData[y * Width + x] = textureData[x, y];
                    }
                }
                Handle?.SubmitData(flattenData);
            }

            return this;
        }
    }
}