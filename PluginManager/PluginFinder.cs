using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PluginManager
{
    /// <summary>
    /// Checks all dll files in the plugin directory to determine if they are
    /// valid IPlugin files. This is done in a separate Namespace so the memory
    /// can be cleared of irrelevant files when finished.
    /// </summary>
    class PluginFinder : MarshalByRefObject
    {
        private ILogger _logger;

        public void SetLogger(ILogger<PluginFinder> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Searches through the given path for any valid plugin dlls.
        /// </summary>
        /// <param name="path">The path where the plugins can be found.</param>
        /// <param name="pluginNamePattern">A filename pattern that all plugin 
        /// dlls will share, such as myApp.plugin.</param>
        /// <returns>List of valid plugin paths.</returns>
        /// <remarks>This needs to be done in its own Namespace, so it can
        /// be unloaded after checking, and the memory can be freed.</remarks>
        public List<String> FindPlugins(String path, string pluginNamePattern)
        {
            var plugins = new List<string>();

            try
            {
                //  Determine all dll files in the given path
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
                _logger?.LogDebug("Loading Plugins: Found " + files.Length + " dlls.");

                //  If the file is a valid plugin, add it to the results
                foreach (FileInfo file in files)
                    if (Path.GetFileName(file.FullName).ToLower().Contains(pluginNamePattern) &&
                        TryLoadingPlugin(file.FullName))
                        plugins.Add(file.FullName);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("does not have the required permission"))
                {
                    _logger?.LogWarning("User does not have permission to access \"" + path + "\"");
                }
                else
                    _logger?.LogError("Error accessing files: ", e);
            }

            _logger?.LogDebug("Loading Plugins: Found " + plugins.Count + " plugins.");
            return plugins;
        }

        /// <summary>
        /// Checks to see if this is a valid plugin.
        /// </summary>
        /// <param name="path">Path to the DLL</param>
        /// <returns>Returns true if this is a valid dll</returns>
        private bool TryLoadingPlugin(string path)
        {
            bool result = false;
            Assembly asm = null;
            _logger?.LogDebug("Attempting to load " + Path.GetFileName(path) + " as a plugin.");

            try
            {
                asm = Assembly.LoadFrom(path);
                foreach (Type t in asm.GetTypes())
                    if (t.GetInterface("IPlugin") != null)
                    {
                        result = true;
                        _logger?.LogDebug("Loading Plugins: Identified " + Path.GetFileName(path) + " as a plugin.");
                        break;
                    }
                    else
                        _logger?.LogDebug(Path.GetFileName(path) + "." + t.Name + " does not appear to be a plugin.");
            }
            #region Exception Handling
            catch (ReflectionTypeLoadException ex)
            {
                //  Common.dll always throws an exception, so skip it.
                if (asm != null && !asm.ManifestModule.Name.ToLower().Equals("common.dll"))
                {
                    _logger?.LogDebug("There was a reflection type load exception caught.");
                    _logger?.LogDebug("Could not load the plugin " + path);
                    _logger?.LogDebug(ex.Message);
                    _logger?.LogDebug(ex.StackTrace);
                    foreach (var item in ex.LoaderExceptions)
                    {
                        _logger?.LogDebug(item.Message.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                //  An Exception most likely means an invalid plugin
                _logger?.LogDebug("Could not load the plugin " + path);
                _logger?.LogDebug(e.Message);
                _logger?.LogDebug(e.StackTrace);
            }
            #endregion

            return result;
        }
    }
}
