using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_sample_preloaded_data_and_questionnaire_is_absent : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_single_PL0001_error()
        {
            var importDataVerifier = CreatePreloadedDataVerifier();

            // Act
            var result = importDataVerifier.VerifyAssignmentsSample(Guid.NewGuid(), 1, new PreloadedDataByFile("1", "1", new string[0], new[] { new string[0] }));

            result.Errors.Should().HaveCount(1);
            result.Errors.First().Code.Should().Be("PL0001");
        }
    }
}
