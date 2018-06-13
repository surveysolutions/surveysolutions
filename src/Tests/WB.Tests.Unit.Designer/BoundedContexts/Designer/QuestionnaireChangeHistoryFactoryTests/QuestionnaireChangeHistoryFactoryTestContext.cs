using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class QuestionnaireChangeHistoryFactoryTestContext
    {
        protected static QuestionnaireChangeHistoryFactory CreateQuestionnaireChangeHistoryFactory(
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage = null,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage = null)
        {
            return
                new QuestionnaireChangeHistoryFactory(
                    questionnaireChangeHistoryStorage ??
                    Mock.Of<IPlainStorageAccessor<QuestionnaireChangeRecord>>(),
                    questionnaireDocumentStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>()
                    );
        }
    }
}
