using System.IO;
using System;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

class PluginManager
{
    public static void InitPlugins()
    {
        string dynamicPluginsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        if (Directory.Exists(dynamicPluginsFolderPath))
        {
            var csFiles = Directory.GetFiles(dynamicPluginsFolderPath, "*.cs");

            foreach (var csFile in csFiles)
            {
                try
                {
                    using (var codeProvider = new CSharpCodeProvider())
                    {
                        CompilerParameters compilerParameters = new CompilerParameters
                        {
                            GenerateInMemory = true,
                            GenerateExecutable = false,
                            
                            ReferencedAssemblies = { "MinesweeperAPI.dll" }
                        };

                        CompilerResults compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, csFile);

                        if (compilerResults.Errors.Count == 0)
                        {
                            Console.WriteLine(compilerResults.CompiledAssembly.GetExportedTypes().Length);
                            foreach (var type in compilerResults.CompiledAssembly.GetExportedTypes())
                            {
                                if (typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract)
                                {
                                    var constructor = type.GetConstructor(Type.EmptyTypes);

                                    if (constructor != null)
                                    {
                                        var plugin = (Plugin)constructor.Invoke(new object[] { });

                                        plugin.SetTunnel(new PluginTunnel(type.Name), type.Name);
                                        plugin.Initialize();

                                        Console.WriteLine($"[Plugins] Initialized {type.Name}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Error loading dynamic plugin {type.Name}: No constructor with MinesweeperGame parameter found.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (CompilerError error in compilerResults.Errors)
                            {
                                Console.WriteLine($"Compilation Error in {csFile}: {error.ErrorText}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading dynamic plugin from {csFile}: {ex.Message}");
                }
            }
        }
    }
}