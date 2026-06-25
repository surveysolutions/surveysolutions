using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Code.Implementation;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteAfterCommand_with_mismatched_revision_questionnaire_id_is_inflating : CommandInflaterTestsContext
    {
        [Test]
        public void should_throw_common_exception()
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser
            {
                Id = actionUserId,
                Email = actionUserEmail
            });
            dbContext.SaveChanges();

            var documentStorage = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            var sourceQuestionnaireRevision = new QuestionnaireRevision(revisionQuestionnaireId, revisionId);

            var command = new PasteAfter(questionnaireId, entityId, pasteAfterId, questionnaireId, entityId, actionUserId)
            {
                SourceQuestionnaireRevision = sourceQuestionnaireRevision
            };

            var commandInflater = CreateCommandInflater(dbContext: dbContext, storage: documentStorage.Object);

            var exception = Assert.Throws<CommandInflaitingException>(() =>
                commandInflater.PrepareDeserializedCommandForExecution(command));

            exception!.ExceptionType.Should().Be(CommandInflatingExceptionType.Common);
            documentStorage.Verify(storage => storage.GetById(It.IsAny<string>()), Times.Never);
        }

        private static readonly Guid questionnaireId = Guid.Parse("13333333333333333333333333333333");
        private static readonly Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static readonly Guid pasteAfterId = Guid.Parse("43333333333333333333333333333333");
        private static readonly Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid revisionQuestionnaireId = Guid.Parse("63333333333333333333333333333333");
        private static readonly Guid revisionId = Guid.Parse("73333333333333333333333333333333");
        private const string actionUserEmail = "test1@example.com";
    }
}
