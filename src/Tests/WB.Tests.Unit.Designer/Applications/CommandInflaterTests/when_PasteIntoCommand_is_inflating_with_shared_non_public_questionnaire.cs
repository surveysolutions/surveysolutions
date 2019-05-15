using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteIntoCommand_is_inflating_with_shared_non_public_questionnaire_but_shared : CommandInflaterTestsContext
    {
        [NUnit.Framework.Test]
        public void should_questionnaire_id_as_provided()
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser
            {
                Id = actionUserId,
                Email = actionUserEmail
            });
            var loggedInUser = Mock.Of<ILoggedInUser>(x => x.Id == actionUserId);

            var questionnaireListViewItem = Create.QuestionnaireListViewItem();
            questionnaireListViewItem.SharedPersons.Add(Create.SharedPerson(actionUserId));
            questionnaireListViewItem.QuestionnaireId = questionnaireId.FormatGuid();
            dbContext.Add(questionnaireListViewItem);

            dbContext.SaveChanges();


            var questionnaire = CreateQuestionnaireDocument(questionnaireId, questionnaiteTitle, ownerId, false);
            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(it.IsAny<string>()) == questionnaire);

            command = new PasteInto(questionnaireId, entityId, questionnaireId, pasteAfterId, entityId, actionUserId);

            commandInflater = CreateCommandInflater(dbContext: dbContext, storage: documentStorage, loggedInUser: loggedInUser);
            BecauseOf();

            command.SourceDocument.Should().NotBeNull();
            command.SourceDocument.PublicKey.Should().Be(questionnaireId);

        }

        private void BecauseOf() =>
            commandInflater.PrepareDeserializedCommandForExecution(command);

        private static CommandInflater commandInflater;
        private static PasteInto command;
        private static Guid questionnaireId = Guid.Parse("13333333333333333333333333333333");

        private static Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static Guid pasteAfterId = Guid.Parse("43333333333333333333333333333333");

        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
    }
}
