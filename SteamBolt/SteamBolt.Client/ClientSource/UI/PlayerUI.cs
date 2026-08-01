using MalignEngine;

namespace SteamBolt;

public class PlayerUI : EntitySystem
{
    private UIManager _uiManager = null!;

    private Container _container;

    public PlayerUI(IServiceContainer serviceContainer, UIManager uiManager) : base(serviceContainer)
    {
        _uiManager = uiManager;

        _container = new Container() { Parent = _uiManager.Root, Width = Length.Fill, Height = Length.Fill };

        var border = new Border() 
        { 
            Parent = _container, 
            Width = Length.Percent(0.9f), 
            Height = Length.Percent(0.08f), 
            Color = new Color(0.5f, 0.5f, 0.5f, 0.7f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        var layout = new Flex() { Parent = border, Width = Length.Fill, Height = Length.Percent(1f) };
    }


}