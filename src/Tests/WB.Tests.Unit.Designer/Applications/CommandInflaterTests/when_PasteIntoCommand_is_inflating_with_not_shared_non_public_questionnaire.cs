using System;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Code.Implementation;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteIntoCommand_is_inflating_with_not_shared_non_public_questionnaire : CommandInflaterTestsContext
    {
        [Test]
        public void should_throw_interview_exception()
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser
            {
                Id = actionUserId,
                Email = actionUserEmail
            });

            var questionnaireListViewItem = Create.QuestionnaireListViewItem();
            questionnaireListViewItem.SharedPersons.Add(Create.SharedPerson(Guid.NewGuid()));
            dbContext.Add(questionnaireListViewItem);

            dbContext.SaveChanges();

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, ownerId, false);
            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(It.IsAny<string>()) == questionnaire);


            command = new PasteInto(questoinnaireId, entityId, pasteAfterId, questoinnaireId, entityId, actionUserId);

            commandInflater = CreateCommandInflater(dbContext: dbContext, storage: documentStorage);

            Assert.Throws<CommandInflaitingException>(() => commandInflater.PrepareDeserializedCommandForExecution(command));
        }


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
