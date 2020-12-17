using System;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql;

namespace WB.Tests.Web.Headquarters.Controllers.GraphTests
{
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [IgnoreLineEndings(true)]
    public class SchemaTest
    {
        [Test]
        public async Task Ensure_schema_isCorrect()
        {
            GraphQLIntegration.AddGraphQL(new ServiceCollection());

            var schema = await GraphQLIntegration.GraphQLBuilder.BuildSchemaAsync();
            var sdl = schema.ToString();
            if (!sdl.EndsWith(Environment.NewLine))
            {
                sdl += Environment.NewLine;
            }
            
            Approvals.Verify(sdl);
        }
    }
}
