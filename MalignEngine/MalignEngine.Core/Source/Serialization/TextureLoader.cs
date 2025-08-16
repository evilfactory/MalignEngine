using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MalignEngine;

public static class TextureLoader
{
    public static ITextureDescriptor Load(string filePath)
    {
        using (Stream file = File.Open(filePath, FileMode.Open))
        {
            return Load(file);
        }
    }

    public static ITextureDescriptor Load(Stream stream)
    {
        TextureDescriptor descriptor = new TextureDescriptor();

        using (var img = Image.Load<Rgba32>(stream))
        {
            descriptor.Width = img.Width;
            descriptor.Height = img.Height;

            img.Mutate(x => x.AutoOrient());
            //img.Mutate(x => x.Flip(FlipMode.Vertical));

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
            Color[] flattenData = new Color[img.Width * img.Height];
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    flattenData[y * img.Width + x] = textureData[x, y];
                }
            }

            descriptor.InitialData = flattenData;
        }

        return descriptor;
    }
}
