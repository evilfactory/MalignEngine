using CommunityToolkit.HighPerformance;
using Silk.NET.OpenGLES;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace MalignEngine
{
    public interface ITexture
    {
        public TextureHandle Handle { get; }
    }

    public class Texture2D : IAsset, ITexture
    {
        public static Texture2D White = new Texture2D(new Color[,] { { Color.White } }, 1, 1);

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public float AspectRatio
        {
            get
            {
                return (float)Width / Height;
            }
        }

        public TextureHandle Handle { get; private set; }

        private Color[,] textureData;

        public Texture2D(uint width, uint height)
        {
            Width = width;
            Height = height;

            CreateHandle();
        }

        public Texture2D(Color[,] data, uint width, uint height)
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
            Handle.SubmitData(flattenData);
        }

        private void CreateHandle()
        {
            var rendering = IoCManager.Resolve<RenderingSystem>();
            Handle = rendering.CreateTextureHandle();
            Handle.Initialize(Width, Height, false);
        }

        public override string ToString()
        {
            return $"Texture2D: ({Width}x{Height})";
        }

        public static IAsset Load(string assetPath)
        {
            using (var img = Image.Load<Rgba32>(assetPath))
            {
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

                Texture2D texture = new Texture2D(textureData, (uint)img.Width, (uint)img.Height);

                return texture;
            }
        }

        public static IAsset CreateDummyAsset()
        {
            return White;
        }
    }
}