using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteIntoCommand_with_historical_revision_is_inflating : CommandInflaterTestsContext
    {
        [Test]
        public void should_load_questionnaire_revision_as_provided()
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser
            {
                Id = actionUserId,
                Email = actionUserEmail
            });
            dbContext.SaveChanges();

            var questionnaire = CreateQuestionnaireDocument(questionnaireId, questionnaireTitle, ownerId);
            var sourceQuestionnaireRevision = new QuestionnaireRevision(questionnaireId, revisionId);

            var questionnaireStorageMock = new Mock<IDesignerQuestionnaireStorage>();
            // Revision-qualified lookup: used to retrieve the historical snapshot
            questionnaireStorageMock
                .Setup(s => s.Get(It.Is<QuestionnaireRevision>(r => r.Revision == revisionId)))
                .Returns(questionnaire);
            // Current-base lookup: used for authorization when a revision is requested
            questionnaireStorageMock
                .Setup(s => s.Get(questionnaireId))
                .Returns(questionnaire);

            command = new PasteInto(questionnaireId, entityId, questionnaireId, entityId, questionnaireId, ownerId, revisionId);

            commandInflater = CreateCommandInflater(dbContext: dbContext, questionnaireStorage: questionnaireStorageMock.Object);

            commandInflater.PrepareDeserializedCommandForExecution(command);

            command.SourceDocument.Should().NotBeNull();
            command.SourceDocument!.PublicKey.Should().Be(questionnaireId);
            questionnaireStorageMock.Verify(
                s => s.Get(It.Is<QuestionnaireRevision>(r => r.Revision == revisionId)),
                Times.Once);
        }

        private static CommandInflater commandInflater;
        private static PasteInto command;
        private static readonly Guid questionnaireId = Guid.Parse("13333333333333333333333333333333");
        private static readonly Guid revisionId = Guid.Parse("63333333333333333333333333333333");
        private static readonly Guid entityId = Guid.Parse("23333333333333333333333333333333");
        private static readonly Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid ownerId = Guid.Parse("53333333333333333333333333333333");
        private const string actionUserEmail = "test1@example.com";
        private const string questionnaireTitle = "questionnaire title";
    }
}
