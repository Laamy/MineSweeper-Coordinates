using System.Windows.Forms;

public interface IPluginInterface
{
    Form GetWin();

    void Log(string message);
}