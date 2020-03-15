using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusCache.Client.Extensions.Tests
{
    [TestClass()]
    public class ClientExtensionsTests
    {
        [TestMethod()]
        public void AddBusCacheClientTest()
        {
            //Arrange
            var mock = new Mock<IConfiguration>();
            ServiceCollection services = new ServiceCollection();
            //Act
            services.AddBusCacheClient(mock.Object);
            //Assert
            Assert.IsTrue(services.Count > 0);
        }
    }
}