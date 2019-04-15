using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteAfterCommand_is_inflating : CommandInflaterTestsContext
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
            dbContext.SaveChanges();

            var questionnaire = CreateQuestionnaireDocument(questionnaireId, questionnaiteTitle, ownerId);

            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(It.IsAny<string>()) == questionnaire);

            command = new PasteAfter(questionnaireId, entityId, pasteAfterId, questionnaireId, entityId, ownerId);

            commandInflater = CreateCommandInflater(dbContext: dbContext, storage: documentStorage);

            BecauseOf();

            command.SourceDocument.PublicKey.Should().Be(questionnaireId);
        }

        private void BecauseOf() =>
            commandInflater.PrepareDeserializedCommandForExecution(command);

        private static CommandInflater commandInflater;
        private static PasteAfter command;
        private static Guid questionnaireId = Guid.Parse("13333333333333333333333333333333");

        private static Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static Guid pasteAfterId = Guid.Parse("43333333333333333333333333333333");

        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
    }
}
