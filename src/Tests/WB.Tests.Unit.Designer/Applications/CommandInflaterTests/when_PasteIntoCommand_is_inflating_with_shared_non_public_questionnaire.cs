using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteIntoCommand_is_inflating_with_shared_non_public_questionnaire_but_shared : CommandInflaterTestsContext
    {
        Establish context = () =>
        {
            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == actionUserId && u.MembershipUser.Email == actionUserEmail));

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, ownerId, false);
            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(it.IsAny<string>()) == questionnaire);

            var shared = new QuestionnaireSharedPersons(questoinnaireId);
            shared.SharedPersons.Add(new SharedPerson() {UserId = actionUserId});

            var sharedPersons =
                Mock.Of<IPlainKeyValueStorage<QuestionnaireSharedPersons>>(
                    s => s.GetById(it.IsAny<string>()) == shared);

            command = new PasteInto(questoinnaireId, entityId, pasteAfterId, questoinnaireId, entityId, actionUserId);

            commandInflater = CreateCommandInflater(membershipUserService, documentStorage, sharedPersons);
        };

        Because of = () =>
            commandInflater.PrepareDeserializedCommandForExecution(command);

        It should_not_be_null = () =>
           command.SourceDocument.ShouldNotBeNull();

        It should_questionnarie_id_as_provided = () =>
            command.SourceDocument.PublicKey.ShouldEqual(questoinnaireId);

        private static CommandInflater commandInflater;
        private static PasteInto command;
        private static Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");

        private static Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static Guid pasteAfterId = Guid.Parse("43333333333333333333333333333333");
        
        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
    }
}