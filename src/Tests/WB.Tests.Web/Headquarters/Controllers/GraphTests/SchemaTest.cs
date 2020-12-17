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
            var schema = await GraphQLIntegration.GetSchema(new ServiceCollection());
            var sdl = schema.Print();
            if (!sdl.EndsWith(Environment.NewLine))
            {
                sdl += Environment.NewLine;
            }
            
            Approvals.Verify(sdl);
        }
    }
}
