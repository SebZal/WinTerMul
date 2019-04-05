using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WinTerMul.Tests.WinTerMul
{
    internal class StartupTests
    {
        [TestClass]
        public class ConfigureServices
        {
            [TestMethod]
            public void ConfigureServices_ServicesConfiguredWithoutError()
            {
                // Arrange
                var services = new ServiceCollection();
                var startup = new Startup();

                // Act
                startup.ConfigureServices(services);

                // Assert
                Assert.IsTrue(services.Count > 0);
            }
        }
    }
}
