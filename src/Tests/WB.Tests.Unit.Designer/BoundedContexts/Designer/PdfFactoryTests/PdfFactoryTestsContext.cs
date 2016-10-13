using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfFactoryTests
{
    public class PdfFactoryTestsContext
    {
        public static PdfFactory CreateFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage = null,
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage = null,
            IPlainStorageAccessor<User> accountsDocumentReader = null,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage = null,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage = null,
            PdfSettings pdfSettings = null)
        {
            return new PdfFactory(
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                questionnaireChangeHistoryStorage: questionnaireChangeHistoryStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireChangeRecord>>(),
                accountsDocumentReader: accountsDocumentReader ?? Mock.Of<IPlainStorageAccessor<User>>(),
                questionnaireListViewItemStorage: questionnaireListViewItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireListViewItem>>(),
                sharedPersonsStorage: sharedPersonsStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(),
                pdfSettings: pdfSettings ?? new PdfSettings(0, 0, 0, 0, 0, 0, 0));
        }
    }
}