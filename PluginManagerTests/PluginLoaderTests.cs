using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PluginManagerTests
{
    [TestClass]
    public class PluginLoaderTests
    {
        /// <summary>
        /// This test should always pass, since it uses the same code to determine the 
        /// directory the code is running in. It is largely added as a way for building
        /// and jumping into the basic code.
        /// </summary>
        [TestMethod]
        public void _TestingSetup()
        {
            var loggerMock = new Mock<ILogger<PluginLoader>>();
            var pluginLoader = new PluginLoader(loggerMock.Object);

            var expectedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.AreEqual(expectedPath, pluginLoader.ApplicationPath);

            var plugins = new List<string>();
            var di = new DirectoryInfo(expectedPath);
            FileInfo[] files = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
                if (Path.GetFileName(file.FullName).ToLower().Contains("implemented"))
                    plugins.Add(file.FullName);

            Debug.WriteLine($"Found {plugins.Count} plugins.");
            Assert.AreEqual(4, plugins.Count, $"Build the test plugin projects and make sure the dlls appear in {expectedPath}.");
        }
    }
}
