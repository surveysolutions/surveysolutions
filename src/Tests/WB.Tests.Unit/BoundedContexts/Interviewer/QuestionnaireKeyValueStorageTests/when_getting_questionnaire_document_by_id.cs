using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.QuestionnaireKeyValueStorageTests
{
    internal class when_getting_questionnaire_document_by_id
    {
        Establish context = () =>
        {
            mockOfAsyncPlainStorage = new Mock<IPlainStorage<QuestionnaireDocumentView>>();
            mockOfAsyncPlainStorage.Setup(x => x.GetById(QuestionnaireId))
                .Returns(new QuestionnaireDocumentView { QuestionnaireDocument = new QuestionnaireDocument()});
            questionnaireKeyValueStorage = Create.Service.QuestionnaireKeyValueStorage(mockOfAsyncPlainStorage.Object);
        };

        Because of = () =>
            questionnaireDocument = questionnaireKeyValueStorage.GetById(QuestionnaireId);

        It should_call_get_by_id_of_async_plain_storage = () =>
            mockOfAsyncPlainStorage.Verify(x=>x.GetById(QuestionnaireId), Times.Once);

        It should_questionnaire_documant_not_be_null = () =>
            questionnaireDocument.ShouldNotBeNull();

        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireKeyValueStorage questionnaireKeyValueStorage;
        private static readonly string QuestionnaireId = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1).ToString();
        private static Mock<IPlainStorage<QuestionnaireDocumentView>> mockOfAsyncPlainStorage;
    }
}