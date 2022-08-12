using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PluginManager
{
    public class PluginLoader
    {
        private ILogger<PluginLoader> _logger;

        public string ApplicationPath { get; }

        public PluginLoader(ILogger<PluginLoader> logger)
        {
            _logger = logger;
            ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Loads all plugins that implement IPluginManagerPlugin from the directory 
        /// where the PluginManager dll has been installed.
        /// </summary>
        /// <returns>Returns a list of loaded IPluginManagerPlugins.</returns>
        public List<IPluginManagerPlugin> LoadPlugins()
        {
            return LoadPlugins(ApplicationPath);
        }

        /// <summary>
        /// Loads all plugins that implement IPluginManagerPlugin from the path parameter.
        /// </summary>
        /// <param name="pluginsPath">The path that will be searched for IPluginManagerPlugins
        /// that match the given plugin name pattern.</param>
        /// <param name="pluginNamePattern">A filename pattern that all plugin dlls will 
        /// share, such as myApp.plugin. By default, this value is set to plugin.</param>
        /// <returns>Returns a list of loaded IPluginManagerPlugins.</returns>
        public List<IPluginManagerPlugin> LoadPlugins(string pluginsPath, string pluginNamePattern = "plugin")
        {
            _logger.LogInformation("Loading installed plugins from " + pluginsPath);
            List<string> foundPlugins = FindPlugins(pluginsPath, pluginNamePattern);

            //  Load the plugins
            var plugins = new List<IPluginManagerPlugin>();
            foreach (string pluginPath in foundPlugins)
            {
                var plugin = LoadPlugin(pluginPath);
                if(plugin != null)
                    plugins.Add(plugin);
            }

            return plugins;
        }

        /// <summary>
        /// Scans the given directory for any files who's name matches the pattern
        /// in the pluginNamePattern and implements the IPluginManagerPlugin
        /// interface, and confirms that these can be loaded.
        /// </summary>
        /// <param name="pluginsPath">The path where plugins will be loaded from.</param>
        /// <param name="pluginNamePattern">A filename pattern that all plugin 
        /// dlls will share, such as myApp.plugin.</param>
        /// <returns></returns>
        private List<string> FindPlugins(string pluginsPath, string pluginNamePattern)
        {
            var plugins = new List<string>();

            try
            {
                // Set up the AppDomainSetup
                var setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

                // Set up the Evidence
                var baseEvidence = AppDomain.CurrentDomain.Evidence;

                //  Create a domain to text for plugins
                AppDomain domain = AppDomain.CreateDomain("PluginLoader", baseEvidence, setup);

                //  Set PluginFinder to run in the new domain (assemblyName, typeName)
                //  CreateInstanceFromAndUnwrap takes as FileStyleUriParser path, while 
                //  CreateInstanceAndUnwrap takes an assembly display name.
                PluginFinder finder = (PluginFinder)domain.CreateInstanceAndUnwrap(
                    typeof(PluginFinder).Assembly.FullName, typeof(PluginFinder).FullName);

                //  Get valid plugins, and then unload the domain to clear up memory
                plugins = finder.FindPlugins(pluginsPath, pluginNamePattern);
                AppDomain.Unload(domain);
            }
            catch (Exception e)
            {
                _logger.LogError("There was an error creating the Plugin Loader.", e);
            }

            return plugins;
        }

        /// <summary>
        /// Attempts to load the plugin with the given path into the DICOM Router.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin to load.</param>
        private IPluginManagerPlugin LoadPlugin(string pluginPath)
        {
            var assembly = Assembly.LoadFrom(pluginPath);
            Type type = null;

            foreach (Type t in assembly.GetTypes())
                if (t.GetInterface("IPluginManagerPlugin") != null)
                    type = t;

            try
            {
                var plugin = (IPluginManagerPlugin)Activator.CreateInstance(type);
                _logger.LogInformation("Loaded " + plugin.Name +
                    " plugin, version " + plugin.Version + ".");

                return plugin;
            }
            catch (NullReferenceException e)
            {
                _logger.LogError("Could not load plugin.", e);
                return null;
            }
        }
    }
}
