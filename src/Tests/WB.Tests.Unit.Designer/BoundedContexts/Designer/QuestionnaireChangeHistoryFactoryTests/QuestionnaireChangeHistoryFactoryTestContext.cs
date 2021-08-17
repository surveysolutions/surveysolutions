using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class QuestionnaireChangeHistoryFactoryTestContext
    {
        protected static QuestionnaireChangeHistoryFactory CreateQuestionnaireChangeHistoryFactory(
            DesignerDbContext dbContext = null,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage = null,
            IQuestionnaireViewFactory viewFactory = null,
            IUserManager userManager = null)
        {
            return
                new QuestionnaireChangeHistoryFactory(
                    dbContext ?? Create.InMemoryDbContext(),
                    questionnaireDocumentStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                    viewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                    userManager ?? Mock.Of<IUserManager>()
                );
        }
    }
}
