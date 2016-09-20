using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class CommandInflaterTestsContext
    {
        protected static CommandInflater CreateCommandInflater(
            IMembershipUserService userHelper = null,
            IPlainKeyValueStorage<QuestionnaireDocument> storage = null,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersons = null)
        {
            return new CommandInflater(
                userHelper ?? Mock.Of<IMembershipUserService>(),
                storage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                sharedPersons ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questoinnaireId, string title, Guid creator, bool isPublic = true)
        {
            return new QuestionnaireDocument() {PublicKey = questoinnaireId, Title = title, CreatedBy = creator, IsPublic = isPublic};
        }
    }
}