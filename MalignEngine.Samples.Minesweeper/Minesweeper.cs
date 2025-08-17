using System.Numerics;
using System.Runtime.InteropServices;

namespace MalignEngine.Samples.Minesweeper;

internal class Tile
{
    public bool IsFlagged = false;
    public bool IsMine = false;
    public int Amount = 0;
    public bool Reveled = false;
    public int EmptyTileIndex;
}

internal enum GameState
{
    Playing,
    Lost,
    Won
}

internal class Minesweeper : IService, IUpdate, IDraw
{
    private IRenderingAPI _renderAPI;
    private IShaderResource _shaderResource;
    private IBufferResource _bufferResource;
    private IVertexArrayResource _vertexArrayResource;
    private IWindowService _windowService;

    private List<ITextureResource> _numberTextures;
    private List<ITextureResource> _emptyTextures;
    private ITextureResource _flagTexture;
    private ITextureResource _mineTexture;
    private ITextureResource _unknownTexture;

    private InputSystem _inputSystem;

    private Matrix4x4 _matrix;

    private Tile[,] _tiles;

    private int _boardSize = 32;

    private GameState _state;

    private bool _firstMove;

    private int _selectedTileX, _selectedTileY;

    private bool CheckWon()
    {
        bool won = true;
        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                if (!_tiles[x, y].IsMine && !_tiles[x, y].Reveled)
                {
                    won = false;
                }
            }
        }

        return won;
    }

    private void RevealAll()
    {
        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                _tiles[x, y].Reveled = true;
            }
        }
    }

    private void GenerateTiles()
    {
        _tiles = new Tile[_boardSize, _boardSize];

        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                _tiles[x, y] = new Tile();

                _tiles[x, y].EmptyTileIndex = Random.Shared.Next(0, 3);

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
                if (x > 0 && y < _boardSize - 1 && _tiles[x - 1, y + 1].IsMine) { amount++; }

                _tiles[x, y].Amount = amount;
            }
        }
    }

    public Minesweeper(IWindowService windowService, IRenderingAPI renderAPI, InputSystem inputSystem)
    {
        _windowService = windowService;
        _renderAPI = renderAPI;
        _inputSystem = inputSystem;

        _firstMove = true;

        _state = GameState.Playing;

        windowService.Size = new Silk.NET.Maths.Vector2D<int>(768, 768);

        GenerateTiles();

        _shaderResource = _renderAPI.CreateShader(new ShaderResourceDescriptor()
        {
            FragmentShaderSource = File.ReadAllText("Content/Frag.glsl"),
            VertexShaderSource = File.ReadAllText("Content/Vert.glsl")
        });


        _numberTextures = new List<ITextureResource>();
        _emptyTextures = new List<ITextureResource>();

        for (int i = 0; i < 8; i++)
        {
            _numberTextures.Add(_renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/proxTile{i + 1}.png")));
        }

        for (int i = 0; i < 3; i++)
        {
            _emptyTextures.Add(_renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/blankTile{i + 1}.png")));
        }

        _flagTexture = _renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/flaggedTile.png"));
        _mineTexture = _renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/activeMine1.png"));
        _unknownTexture = _renderAPI.CreateTexture(TextureLoader.Load($"Content/Textures/unknownTile.png"));

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

            RevealNearby(x + 1, y + 1);
            RevealNearby(x - 1, y + 1);
            RevealNearby(x - 1, y + 1);
            RevealNearby(x + 1, y - 1);
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
            //ctx.SetTexture(0, _textureResource);

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

                    if (_inputSystem.IsMouseInsideRectangle(new Vector2(x * scale.X, y * scale.Y), scale, false) && _state == GameState.Playing)
                    {
                        _shaderResource.Set("uColor", Color.LightGray);

                        _selectedTileX = x;
                        _selectedTileY = y;
                    }
                    else
                    {
                        _shaderResource.Set("uColor", Color.White);
                    }

                    if (_tiles[x, y].Reveled)
                    {
                        if (_tiles[x, y].IsMine)
                        {
                            ctx.SetTexture(0, _mineTexture);
                        }
                        else if (_tiles[x, y].Amount > 0)
                        {
                            ctx.SetTexture(0, _numberTextures[_tiles[x, y].Amount - 1]);
                        }
                        else
                        {
                            ctx.SetTexture(0, _emptyTextures[_tiles[x, y].EmptyTileIndex]);
                        }
                    }
                    else
                    {
                        if (_tiles[x, y].IsFlagged)
                        {
                            ctx.SetTexture(0, _flagTexture);
                        }
                        else
                        {
                            ctx.SetTexture(0, _unknownTexture);
                        }
                    }

                    if (_state == GameState.Won)
                    {
                        _shaderResource.Set("uColor", Color.Green);
                    }
                    else if (_state == GameState.Lost)
                    {
                        _shaderResource.Set("uColor", Color.Red);
                    }

                    ctx.DrawArrays(_bufferResource, _vertexArrayResource, 6);
                }
            }
        });
    }

    public void OnUpdate(float deltaTime)
    {
        if (_inputSystem.IsKeyHeld(Key.R))
        {
            GenerateTiles();
            _state = GameState.Playing;
            _firstMove = false;
            return;
        }

        int x = _selectedTileX;
        int y = _selectedTileY;

        if (_inputSystem.IsMouseButtonHeld(1))
        {
            _tiles[x, y].IsFlagged = !_tiles[x, y].IsFlagged;
        }

        if (_inputSystem.IsMouseButtonHeld(0) && !_tiles[x, y].IsFlagged)
        {
            if (_firstMove)
            {
                _tiles[x, y].IsMine = false;
            }

            if (_tiles[x, y].IsMine)
            {
                _state = GameState.Lost;
                RevealAll();
            }
            else
            {
                RevealNearby(x, y);
                _tiles[x, y].Reveled = true;

                if (CheckWon())
                {
                    _state = GameState.Won;
                    RevealAll();
                }
            }

            _firstMove = false;
        }
    }
}
