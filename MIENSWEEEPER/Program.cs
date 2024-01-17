using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // load the plugin shared memory
        AppDomain.CurrentDomain.AssemblyResolve += OnResolve;

        Application.Run(new MinesweeperGame ());
    }

    static Assembly OnResolve(object sender, ResolveEventArgs args)
    {
        string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", args.Name.Split(',')[0] + ".dll");

        Console.WriteLine($"Loading shared resource {assemblyPath}");

        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        return null;
    }
}