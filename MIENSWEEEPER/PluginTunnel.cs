using System;
using System.Windows.Forms;

public class PluginTunnel : IPluginInterface
{
    string plugName;

    public PluginTunnel(string pluginName)
    {
        plugName = pluginName;
    }

    public void Log(string message)
    {
        Console.WriteLine($"[{plugName}] {message}");
    }

    public Form GetWin()
    {
        return MinesweeperGame.Instance;
    }

    public void Dispose()
    {
        plugName = null;
    }

    public void RedrawGame()
    {
        MinesweeperGame.Instance.RedrawGame();
    }
}