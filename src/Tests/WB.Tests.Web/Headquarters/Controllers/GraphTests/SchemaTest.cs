using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using HotChocolate;
using NUnit.Framework;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql;

namespace WB.Tests.Web.Headquarters.Controllers.GraphTests
{
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [IgnoreLineEndings(true)]
    public class SchemaTest
    {
        [Test]
        public void Ensure_schema_isCorrect()
        {
            var schema = GraphQLIntegration.HeadquartersSchema;

            var sdl = schema.ToString();
            if (!sdl.EndsWith(Environment.NewLine))
            {
                sdl += Environment.NewLine;
            }
            
            Approvals.Verify(sdl);
        }
    }
}
