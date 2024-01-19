using System;
using System.Drawing;

public abstract class Plugin
{
    private string _name;
    public IPluginInterface PluginTunnel { get; private set; }

    public Plugin() { }

    public string Name
    { get => _name; }

    public void SetTunnel(IPluginInterface Logger, string name)
    {
        PluginTunnel = Logger;
        _name = name;
    }

    public virtual void Initialize() { }

    public virtual void OnGameStart() { }

    public virtual void OnGameEnd() { }

    public virtual void OnTileClicked(Tuple<int, int> clicked, Tile tile) { }

    public virtual void OnTileRightClicked(Tuple<int, int> clicked, Tile tile) { }

    public virtual void OnTileRender(Graphics g, int x, int y, Tile tile, ref bool cancel)
    {
        //*cancel = *cancel;
    }
}