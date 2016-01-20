using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_storing_questionnaire : InterviewerQuestionnaireAccessorTestsContext
    {
        Establish context = () =>
        {
            var serializer = Mock.Of<ISerializer>(x => x.Deserialize<QuestionnaireDocument>(Moq.It.IsAny<string>()) == questionnaireDocument);
            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                serializer: serializer,
                questionnaireModelViewRepository: mockOfQuestionnaireModelViewRepository.Object,
                questionnaireViewRepository: mockOfQuestionnaireViewRepository.Object,
                questionnaireDocumentRepository: mockOfQuestionnaireDocumentRepository.Object);
        };

        Because of = async () =>
            await interviewerQuestionnaireAccessor.StoreQuestionnaireAsync(questionnaireIdentity, questionnaireDocumentAsString, isCensusQuestionnaire);

        It should_store_questionnaire_document_view_to_plain_storage = () =>
            mockOfQuestionnaireDocumentRepository.Verify(x => x.StoreAsync(Moq.It.IsAny<QuestionnaireDocumentView>()), Times.Once);

        It should_store_questionnaire_model_view_to_plain_storage = () =>
            mockOfQuestionnaireModelViewRepository.Verify(x => x.StoreAsync(Moq.It.IsAny<QuestionnaireModelView>()), Times.Once);

        It should_store_questionnaire_view_to_plain_storage = () =>
            mockOfQuestionnaireViewRepository.Verify(x => x.StoreAsync(Moq.It.IsAny<QuestionnaireView>()), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IAsyncPlainStorage<QuestionnaireDocumentView>> mockOfQuestionnaireDocumentRepository = new Mock<IAsyncPlainStorage<QuestionnaireDocumentView>>();
        private static readonly Mock<IAsyncPlainStorage<QuestionnaireModelView>> mockOfQuestionnaireModelViewRepository = new Mock<IAsyncPlainStorage<QuestionnaireModelView>>();
        private static readonly Mock<IAsyncPlainStorage<QuestionnaireView>> mockOfQuestionnaireViewRepository = new Mock<IAsyncPlainStorage<QuestionnaireView>>();
        private const bool isCensusQuestionnaire = true;
        private const string questionnaireDocumentAsString = "questionnaire document";
        private static readonly QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { Title = "title of questionnaire"};
        private static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
