using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using WB.Services.Export.Host;

namespace WB.Services.Export.Tests
{
    public class StartupTests
    {
        private readonly WebApplicationFactory<Startup> factory;

        public StartupTests()
        {
            factory = new WebApplicationFactory<Startup>();
        }

        //[Test]
        //public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        //{
        //    // Arrange
            
        //    var client = factory.CreateClient();

        //    // Act
        //    var response = await client.GetAsync("/metrics");

        //    // Assert
        //    response.EnsureSuccessStatusCode(); // Status Code 200-299
        //}
    }
}
