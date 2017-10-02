using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [TestFixture]
    internal class when_verifying_panel_preloaded_data_and_file_with_questionnaire_level_is_absent : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_error_with_code_PL0040()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId);
            questionnaireDocument.Title = "title";

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaireDocument);
            var assignmentData = new[]
            {
                Create.Entity.PreloadedDataByFile(fileName: "roster1"),
                Create.Entity.PreloadedDataByFile(fileName: "roster2")
            };

            importDataVerifier.VerifyPanelFiles(Guid.NewGuid(), 1, assignmentData, status);

            status.VerificationState.Errors.Count().ShouldEqual(1);
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0040");
        }
    }
}
