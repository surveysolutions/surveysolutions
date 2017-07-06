using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_unmapped_file : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            importDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { CreatePreloadedDataByFile() }, status);
        
        It should_result_has_1_error = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0004_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0004");

        It should_return_reference_with_File_type = () =>
            status.VerificationState.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.File);

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
    }
}
