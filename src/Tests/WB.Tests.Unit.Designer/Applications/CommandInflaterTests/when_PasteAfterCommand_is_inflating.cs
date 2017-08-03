using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Code.Implementation;

using it = Moq.It;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteAfterCommand_is_inflating : CommandInflaterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == actionUserId && u.MembershipUser.Email == actionUserEmail));

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, ownerId);

            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(it.IsAny<string>()) == questionnaire);

            command = new PasteAfter(questoinnaireId, entityId, pasteAfterId, questoinnaireId, entityId, ownerId);

            commandInflater = CreateCommandInflater(membershipUserService, documentStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            commandInflater.PrepareDeserializedCommandForExecution(command);

        [NUnit.Framework.Test] public void should_not_be_null () =>
           command.SourceDocument.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_questionnarie_id_as_provided () =>
            command.SourceDocument.PublicKey.ShouldEqual(questoinnaireId);

        private static CommandInflater commandInflater;
        private static PasteAfter command;
        private static Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");

        private static Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static Guid pasteAfterId = Guid.Parse("43333333333333333333333333333333");
        
        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
    }
}