using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Integration.APITests;

public class SwaggerTests
{
    [Test]
    [Ignore("Fix of init is required")]
    public async Task when_generating_schema()
    {
        SwaggerGenOptions options = new SwaggerGenOptions();
        SwaggerIntegration.AddOptions(options);

        //issue happened in apiDescriptionsProvider
        IApiDescriptionGroupCollectionProvider apiDescriptionsProvider = Mock.Of<IApiDescriptionGroupCollectionProvider>();
        ISchemaGenerator schemaGenerator = Mock.Of<ISchemaGenerator>();
        IAuthenticationSchemeProvider authenticationSchemeProvider = Mock.Of<IAuthenticationSchemeProvider>();

        var generator = new SwaggerGenerator(
            options.SwaggerGeneratorOptions,
            apiDescriptionsProvider,
            schemaGenerator,
            authenticationSchemeProvider
        );

        var result = await generator.GetSwaggerAsync("v1", null, "/primary");

        //Assert.That(result, Is.EqualTo(expected));
    }
}
