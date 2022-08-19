using System.Diagnostics;
using System.Reflection;

namespace Test3.Plugin.NotImplemented
{
    /// <summary>
    /// This plugin should not be loaded because it
    /// does not implement IPluginManagerPlugin.
    /// </summary>
    public class Test3
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

        public string Name { get; } = "Plugin Test 3";
    }
}
