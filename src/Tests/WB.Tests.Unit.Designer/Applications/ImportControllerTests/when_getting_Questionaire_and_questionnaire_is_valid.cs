using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_questionnaire_is_valid : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Setup.DesignerEngineVersionService();
           
            var questionnaireVerifier = Mock.Of<IQuestionnaireVerifier>(
                _ => _.CheckForErrors(Moq.It.IsAny<QuestionnaireView>()) == new QuestionnaireVerificationMessage[0]);

            var expressionProcessorGenerator = Mock.Of<IExpressionProcessorGenerator>(
                _ =>
                    _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>(),
                        out generatedAssembly) == Create.GenerationResult(true));

            var serializer = Mock.Of<ISerializer>(
                x => x.Serialize(Moq.It.IsAny<QuestionnaireDocument>()) == serializedQuestionnaire);

            var stringCompressorMock =
                Mock.Of<IStringCompressor>(
                    x => x.CompressString(serializedQuestionnaire) == compressedSerializedQuestionnaire);

            importController = CreateImportController(
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier,
                expressionProcessorGenerator: expressionProcessorGenerator,
                serializer: serializer,
                zipUtils: stringCompressorMock);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaireCommunicationPackage = importController.Questionnaire(request);

        [NUnit.Framework.Test] public void should_return_not_null_responce () =>
            questionnaireCommunicationPackage.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_generated_assembly () =>
            questionnaireCommunicationPackage.QuestionnaireAssembly.Should().Be(generatedAssembly);

        [NUnit.Framework.Test] public void should_return_compressed_serialized_questionnaire () =>
            questionnaireCommunicationPackage.Questionnaire.Should().Be(compressedSerializedQuestionnaire);

        private static ImportV2Controller importController;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
        private static QuestionnaireCommunicationPackage questionnaireCommunicationPackage;
        private static string generatedAssembly = "test";
        private static readonly string serializedQuestionnaire = "serialized questionnaire";
        private static readonly string compressedSerializedQuestionnaire = "compressed serialized questionnaire";
    }
}
