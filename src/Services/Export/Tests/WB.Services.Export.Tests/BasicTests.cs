using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using WB.Services.Export.Host;

namespace WB.Services.Export.Tests
{
    public class BasicTests
    {
        private readonly WebApplicationFactory<WB.Services.Export.Host.Startup> _factory;

        public BasicTests()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [Test]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            // Arrange
            
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/metrics");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299

        }
    }
}
