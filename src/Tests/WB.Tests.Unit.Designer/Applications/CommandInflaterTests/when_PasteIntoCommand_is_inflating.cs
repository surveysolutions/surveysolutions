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
    internal class when_PasteIntoCommand_is_inflating : CommandInflaterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser
            {
                Id = actionUserId,
                Email = actionUserEmail
            });
            dbContext.SaveChanges();

            var questionnaire = CreateQuestionnaireDocument(questoinnaireId, questionnaiteTitle, ownerId);

            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                    => storage.GetById(It.IsAny<string>()) == questionnaire);

            command = new PasteInto(questoinnaireId, entityId, pasteAfterId, questoinnaireId, entityId, ownerId);

            commandInflater = CreateCommandInflater(dbContext: dbContext, storage: documentStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            commandInflater.PrepareDeserializedCommandForExecution(command);

        [NUnit.Framework.Test] public void should_not_be_null () =>
           command.SourceDocument.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_questionnarie_id_as_provided () =>
            command.SourceDocument.PublicKey.Should().Be(questoinnaireId);

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
