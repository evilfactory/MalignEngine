using nkast.Aether.Physics2D.Collision.Shapes;
using System.Text;

namespace MalignEngine;

public class ShaderAssetLoader : IService, IAssetLoader
{
    private enum ShaderSection
    {
        None,
        Shared,
        Fragment,
        Vertex
    }
    private sealed class ShaderChunk
    {
        public int FirstLine { get; set; }
        public StringBuilder Source { get; } = new();
    }

    public IReadOnlyCollection<Type> AssetTypes => [typeof(ShaderAsset)];

    private IRenderingAPI _renderingAPI;

    public ShaderAssetLoader(IRenderingAPI renderingAPI)
    {
        _renderingAPI = renderingAPI;
    }

    public IAsset Load(Stream stream)
    {
        Dictionary<ShaderSection, ShaderChunk> sections = [];
        ShaderSection current = ShaderSection.None;

        using StreamReader reader = new(stream, Encoding.UTF8);

        int lineNumber = 1;

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine()!;

            if (line.StartsWith("#pragma"))
            {
                current = ParsePragma(line);
                lineNumber++;
                continue;
            }

            if (!sections.TryGetValue(current, out ShaderChunk? chunk))
            {
                chunk = new ShaderChunk
                {
                    FirstLine = lineNumber
                };

                sections[current] = chunk;
            }

            chunk.Source.AppendLine(line);

            lineNumber++;
        }

        var descriptor = new ShaderResourceDescriptor(BuildSource(ShaderSection.Vertex, sections), BuildSource(ShaderSection.Fragment, sections));
        var shaderResource = _renderingAPI.CreateShader(descriptor);
        return new ShaderAsset(shaderResource);
    }

    private string BuildSource(ShaderSection stage, Dictionary<ShaderSection, ShaderChunk> sections)
    {
        StringBuilder builder = new();

        if (sections.TryGetValue(ShaderSection.Shared, out ShaderChunk? shared))
        {
            builder.AppendLine($"#line {shared.FirstLine}");
            builder.Append(shared.Source);
            builder.AppendLine();
        }

        if (sections.TryGetValue(stage, out ShaderChunk? stageChunk))
        {
            builder.AppendLine($"#line {stageChunk.FirstLine}");
            builder.Append(stageChunk.Source);
        }

        return builder.ToString();
    }

    private static ShaderSection ParsePragma(string line)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts[1] switch
        {
            "shared" => ShaderSection.Shared,
            "vertex" => ShaderSection.Vertex,
            "fragment" => ShaderSection.Fragment,
            _ => ShaderSection.None
        };
    }

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}