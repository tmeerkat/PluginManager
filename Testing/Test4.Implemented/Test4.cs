using PluginManager;
using System.Diagnostics;
using System.Reflection;

namespace Test4.Implemented
{
    /// <summary>
    /// This plugin should not be loaded because it does not match 
    /// the nameing convention.
    /// </summary>
    public class Test4 : IPluginManagerPlugin
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

        public string Name { get; } = "Plugin Test 4";
    }
}
