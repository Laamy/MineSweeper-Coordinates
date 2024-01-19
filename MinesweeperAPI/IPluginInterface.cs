using System;
using System.Windows.Forms;

public interface IPluginInterface : IDisposable
{
    Form GetWin();

    void Log(string message);

    void RedrawGame();
}