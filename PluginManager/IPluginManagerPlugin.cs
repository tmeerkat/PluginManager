namespace PluginManager
{
    public interface IPluginManagerPlugin
    {
        string Version { get; }
        string Name { get; }
    }
}
