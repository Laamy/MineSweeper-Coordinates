using System;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.Run(new MinesweeperGame ());
    }
}