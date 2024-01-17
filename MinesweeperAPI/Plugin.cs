using System;

public abstract class Plugin
{
    private string _name;
    protected IPluginInterface PluginTunnel;

    public Plugin() { }

    public string Name { get { return _name; } }

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

    public void SetTunnel(object value)
    {
        throw new NotImplementedException();
    }
}