using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Code.Implementation;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class CommandInflaterTestsContext
    {
        protected static CommandInflater CreateCommandInflater(
            IMembershipUserService userHelper = null,
            IPlainKeyValueStorage<QuestionnaireDocument> storage = null,
            IPlainStorageAccessor<QuestionnaireListViewItem> listViewItems = null)
        {
            return new CommandInflater(
                userHelper ?? Mock.Of<IMembershipUserService>(),
                storage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                listViewItems ?? Mock.Of<IPlainStorageAccessor<QuestionnaireListViewItem>>(),
                Create.AccountRepository());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questoinnaireId, string title, Guid creator, bool isPublic = true)
        {
            return new QuestionnaireDocument() {PublicKey = questoinnaireId, Title = title, CreatedBy = creator, IsPublic = isPublic};
        }
    }
}
