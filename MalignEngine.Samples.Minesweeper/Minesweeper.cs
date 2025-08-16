using System.Numerics;
using System.Runtime.InteropServices;

namespace MalignEngine.Samples.Minesweeper;

internal class Tile
{
    public bool IsFlagged = false;
    public bool IsMine = false;
    public int Amount = 0;
    public bool Reveled = false;
}

internal class Minesweeper : IService, IDraw
{
    private IRenderingAPI _renderAPI;
    private IShaderResource _shaderResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IWindowService _windowService;
    private List<ITextureResource> _textureResources;

    private InputSystem _inputSystem;

    private Matrix4x4 _matrix;

    private Tile[,] _tiles;

    private int _boardSize = 32;

    private bool _firstMove;

    public Minesweeper(IWindowService windowService, IRenderingAPI renderAPI, InputSystem inputSystem)
    {
        _windowService = windowService;
        _renderAPI = renderAPI;
        _inputSystem = inputSystem;

        _textureResources = new List<ITextureResource>();
        _firstMove = true;

        _tiles = new Tile[_boardSize, _boardSize];

        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                _tiles[x, y] = new Tile();

                if (Random.Shared.NextDouble() > 0.85f)
                {
                    _tiles[x, y].IsMine = true;
                }
            }
        }

        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                if (_tiles[x, y].IsMine) { continue; }
                int amount = 0;

                if (x > 0 && _tiles[x - 1, y].IsMine) { amount++; }
                if (x < _boardSize - 1 && _tiles[x + 1, y].IsMine) { amount++; }
                if (y > 0 && _tiles[x, y - 1].IsMine) { amount++; }
                if (y < _boardSize - 1 && _tiles[x, y + 1].IsMine) { amount++; }

                if (x > 0 && y > 0 && _tiles[x - 1, y - 1].IsMine) { amount++; }
                if (x < _boardSize - 1 && y < _boardSize - 1 && _tiles[x + 1, y + 1].IsMine) { amount++; }
                if (x < _boardSize - 1 && y > 0 && _tiles[x + 1, y - 1].IsMine) { amount++; }
                if (x > 0 &&  y < _boardSize - 1 && _tiles[x - 1, y + 1].IsMine) { amount++; }

                _tiles[x, y].Amount = amount;
            }
        }

        _shaderResource = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/TestFrag.glsl"),
            VertexShaderSource = File.ReadAllText("Content/TestVert.glsl")
        });

        for (int i = 0; i < 8; i++)
        {
            _textureResources.Add(_renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/{i}.png")));
        }

        var desc = new VertexArrayDescriptor();
        desc.AddAttribute("Color", 0, VertexAttributeType.Float, 3, false);
        desc.AddAttribute("UV", 1, VertexAttributeType.Float, 2, false);
        _vertexArrayResource = _renderAPI.CreateVertexArray(desc);

        float[] imageData = new float[]
        {
            0, 0, 0f,     0f, 0f, // Bottom-left
             1, 0, 0f,     1f, 0f, // Bottom-right
             1,  1, 0f,     1f, 1f, // Top-right

            // Triangle 2
            0, 0, 0f,     0f, 0f, // Bottom-left
             1,  1, 0f,     1f, 1f, // Top-right
            0,  1, 0f,     0f, 1f  // Top-left
        };

        ReadOnlySpan<float> floatSpan = imageData;
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(floatSpan);
        byte[] bytes = byteSpan.ToArray();

        _bufferResource = _renderAPI.CreateBuffer(new BufferResourceDescriptor(BufferObjectType.Vertex, BufferUsageType.Static, bytes));
    }

    private void RevealNearby(int x, int y)
    {
        if (x < 0 || y < 0) { return; }
        if (x > _boardSize - 1 || y > _boardSize - 1) { return; }

        if (_tiles[x, y].Reveled || _tiles[x, y].IsMine) { return; }
      
        if (_tiles[x, y].Amount == 0)
        {
            _tiles[x, y].Reveled = true;
            RevealNearby(x + 1, y);
            RevealNearby(x - 1, y);
            RevealNearby(x, y + 1);
            RevealNearby(x, y - 1);
        }
        else
        {
            _tiles[x, y].Reveled = true;
        }
    }

    public void OnDraw(float deltaTime)
    {
        _matrix = Matrix4x4.CreateOrthographicOffCenter(0, _windowService.Size.X, _windowService.Size.Y, 0f, 0.0001f, 100f);

        _renderAPI.Submit(ctx =>
        {
            ctx.SetFrameBuffer(null, _windowService.FrameSize.X, _windowService.FrameSize.Y);
            ctx.Clear(Color.White);

            ctx.SetShader(_shaderResource);
            _shaderResource.Set("uProjection", _matrix);
            ctx.SetTexture(0, _textureResource);

            int amount = _boardSize;
            for (int x = 0; x < amount; x++)
            {
                for (int y = 0; y < amount; y++)
                {
                    Vector2 scale = new Vector2(_windowService.FrameSize.X / amount, _windowService.FrameSize.X / amount);
                    Matrix4x4 matrix = Matrix4x4.Identity;
                    matrix = matrix * Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, 1f));
                    matrix = matrix * Matrix4x4.CreateTranslation(x * scale.X, y * scale.Y, 0);
                    _shaderResource.Set("uModel", matrix);

                    if (_inputSystem.IsMouseInsideRectangle(new Vector2(x * scale.X, y * scale.Y), scale, false))
                    {
                        _shaderResource.Set("uColor", Color.Red);

                        if (_inputSystem.IsMouseButtonPressed(1))
                        {
                            _tiles[x, y].IsFlagged = !_tiles[x, y].IsFlagged;
                        }

                        if (_inputSystem.IsMouseButtonPressed(0))
                        {
                            if (_tiles[x, y].IsMine)
                            {
                                if (_firstMove)
                                {
                                    _tiles[x, y].IsMine = false;
                                }
                                else
                                {
                                    throw new Exception("gah");
                                }
                            }

                            _firstMove = false;

                            RevealNearby(x, y);
                            _tiles[x, y].Reveled = true;
                        }
                    }
                    else
                    {
                        _shaderResource.Set("uColor", Color.White);
                    }

                    if (_tiles[x, y].IsFlagged)
                    {
                        _shaderResource.Set("uColor", Color.Blue);
                    }

                    if (_tiles[x, y].Reveled)
                    {
                        _shaderResource.Set("uColor", Color.Black);
                        
                        if (_tiles[x, y].Amount > 0)
                        {
                            _shaderResource.Set("uColor", Color.DarkGreen);
                        }
                    }

                    ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);
                }
            }
        });
    }
}
