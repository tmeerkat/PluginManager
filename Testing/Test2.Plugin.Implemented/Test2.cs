using PluginManager;
using System.Diagnostics;
using System.Reflection;

namespace Test2.Plugin.Implemented
{
    /// <summary>
    /// This plugin should be loaded.
    /// </summary>
    public class Test2 : IPluginManagerPlugin
    {
        public string Version
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        public string Name { get; } = "Plugin Test 2";
    }
}
