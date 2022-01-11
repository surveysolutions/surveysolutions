using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class CommandInflaterTestsContext
    {
        protected static CommandInflater CreateCommandInflater(
            IPlainKeyValueStorage<QuestionnaireDocument> storage = null,
            IPlainStorageAccessor<QuestionnaireListViewItem> listViewItems = null,
            IClassificationsStorage classificationsStorage = null,
            DesignerDbContext dbContext = null,
            ILoggedInUser loggedInUser = null)
        {
            return new CommandInflater(
                storage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                dbContext ?? Create.InMemoryDbContext(),
                classificationsStorage ?? Mock.Of<IClassificationsStorage>(),
                loggedInUser ?? Mock.Of<ILoggedInUser>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questoinnaireId, string title, Guid creator, bool isPublic = true)
        {
            return new QuestionnaireDocument() {PublicKey = questoinnaireId, Title = title, CreatedBy = creator, IsPublic = isPublic};
        }
    }
}
