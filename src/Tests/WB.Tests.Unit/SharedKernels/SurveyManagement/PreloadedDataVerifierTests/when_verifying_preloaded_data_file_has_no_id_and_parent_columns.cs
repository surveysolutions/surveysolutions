using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_no_id_and_parent_columns : PreloadedDataVerifierTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
            BecauseOf();
        }

        private void BecauseOf() => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(CreatePreloadedDataByFile(new string[0], null, QuestionnaireCsvFileName)), status);

        [NUnit.Framework.Test] public void should_result_has_1_error () =>
            status.VerificationState.Errors.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_PL0007_error () =>
            status.VerificationState.Errors.First().Code.Should().Be("PL0007");

        [NUnit.Framework.Test] public void should_return_second_PL0007_error () =>
            status.VerificationState.Errors.Last().Code.Should().Be("PL0007");

        [NUnit.Framework.Test] public void should_return_reference_of_first_error_with_Column_type () =>
            status.VerificationState.Errors.First().References.First().Type.Should().Be(PreloadedDataVerificationReferenceType.Column);

        [NUnit.Framework.Test] public void should_return_reference_of_second_error_with_Column_type () =>
            status.VerificationState.Errors.Last().References.First().Type.Should().Be(PreloadedDataVerificationReferenceType.Column);

        
        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
    }
}
