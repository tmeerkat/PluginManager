using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginManager;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PluginManagerTests
{
    [TestClass]
    public class PluginLoaderTests
    {
        public static string RunPath
        {
            get
            {
                if(string.IsNullOrEmpty(_path))
                    _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return _path;
            }
        }

        private static string _emptyPath = $"{RunPath}/Empty";
        private static string _badPluginPath = $"{RunPath}/BadPlugins";
        private static string _singlePath = $"{RunPath}/Single";

        private static string _path;
        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var test1 = "Test1.Plugin.Implemented.dll";
            //var test2 = "Test2.Plugin.Implemented.dll";
            var test3 = "Test3.Plugin.NotImplemented.dll";
            var test4 = "Test4.Implemented.dll";

            Directory.CreateDirectory(_emptyPath);
            Directory.CreateDirectory(_badPluginPath);
            Directory.CreateDirectory(_singlePath);

            File.Copy(Path.Combine(RunPath, test3), Path.Combine(_badPluginPath, test3));
            File.Copy(Path.Combine(RunPath, test4), Path.Combine(_badPluginPath, test4));
            File.Copy(Path.Combine(RunPath, test1), Path.Combine(_singlePath, test1));
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.Delete($"{path}/Empty", true);
            Directory.Delete($"{path}/BadPlugins", true);
            Directory.Delete($"{path}/Single", true);
        }

        /// <summary>
        /// This test confirms that files are in the correct folders and
        /// can be accessed for testing.
        /// </summary>
        [TestMethod]
        public void _TestingSetup()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var expectedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.AreEqual(expectedPath, pluginLoader.ApplicationPath);

            var numberOfPlugins = GetPluginCount(expectedPath);
            Assert.AreEqual(4, numberOfPlugins, $"Build the test plugin projects and make sure the dlls appear in {expectedPath}.");
        }

        [TestMethod]
        public void PluginLoaderAllPluginsTest()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var plugins = pluginLoader.LoadPlugins("plugin");
            Assert.AreEqual(2, plugins.Count);
        }

        [TestMethod]
        public void PluginLoaderBadPatternTest()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var plugins = pluginLoader.LoadPlugins("does_not_exist");
            Assert.AreEqual(0, plugins.Count);
        }

        [TestMethod]
        public void PluginLoaderNoFilesTest()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var plugins = pluginLoader.LoadPlugins(_emptyPath, "plugin");
            Assert.AreEqual(0, plugins.Count);
        }

        [TestMethod]
        public void PluginLoaderNotValidPluginTest()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var plugins = pluginLoader.LoadPlugins(_badPluginPath, "plugin");
            Assert.AreEqual(0, plugins.Count);
        }

        [TestMethod]
        public void PluginLoaderOnePluginTest()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PluginLoader>();
            var pluginLoader = new PluginLoader(logger);

            var plugins = pluginLoader.LoadPlugins(_singlePath, "plugin");
            Assert.AreEqual(1, plugins.Count);
        }

        #region Helper Methods

        /// <summary>
        /// Counts the number of test plugins located in the given path. The returned
        /// number need only contain "implemented" in the library name, since some
        ///  required plugins do not implement IPluginManagerPlugin.
        /// </summary>
        /// <param name="path">Directory to look for plugins.</param>
        /// <returns>The number of files with "implemented" in their name found in
        /// the given directory.</returns>
        public int GetPluginCount(string path)
        {
            var plugins = new List<string>();
            var di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
                if (Path.GetFileName(file.FullName).ToLower().Contains("implemented"))
                    plugins.Add(file.FullName);

            Debug.WriteLine($"Found {plugins.Count} plugins in {path}.");
            return plugins.Count;
        }

        #endregion
    }
}
