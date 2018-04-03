using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_panel_preloaded_data_and_questionnaire_is_absent : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_single_PL0024_error()
        {
            var importDataVerifier = CreatePreloadedDataVerifier();

            // Act
            importDataVerifier.VerifyPanelFiles(Guid.NewGuid(), 1, Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]), status);

            // Assert
            status.VerificationState.Errors.Should().HaveCount(1);
            status.VerificationState.Errors.First().Code.Should().Be("PL0024");
        }
    }
}
