using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PluginManager;
using System;
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
        public void TestMethod1()
        {
            var loggerMock = new Mock<ILogger<PluginLoader>>();
            var pluginLoader = new PluginLoader(loggerMock.Object);

            var expected = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.AreEqual(expected, pluginLoader.ApplicationPath);
        }
    }
}
