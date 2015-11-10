using Moq;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using System;

namespace WB.Tests.Unit.Applications.Designer.CommandPostProcessorTests
{
    internal class CommandInflaterTestsContext
    {
        protected static CommandInflater CreateCommandInflater(
            IMembershipUserService userHelper = null,
            IReadSideKeyValueStorage<QuestionnaireDocument> storage = null)
        {
            return new CommandInflater(
                userHelper ?? Mock.Of<IMembershipUserService>(),
                storage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questoinnaireId, string title, Guid creator)
        {
            return new QuestionnaireDocument() {PublicKey = questoinnaireId, Title = title, CreatedBy = creator, IsPublic = true};
        }
    }
}