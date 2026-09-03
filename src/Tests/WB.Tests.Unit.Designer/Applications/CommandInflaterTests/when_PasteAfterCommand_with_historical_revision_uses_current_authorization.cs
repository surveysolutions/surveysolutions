using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Code.Implementation;

namespace WB.Tests.Unit.Designer.Applications.CommandInflaterTests
{
    internal class when_PasteAfterCommand_with_historical_revision_uses_current_authorization : CommandInflaterTestsContext
    {
        [Test]
        public void should_deny_access_when_questionnaire_was_public_at_revision_but_is_now_private()
        {
            // Historical snapshot says questionnaire was public, but current snapshot is private.
            var historicalSnapshot = CreateQuestionnaireDocument(questionnaireId, title, ownerId, isPublic: true);
            var currentSnapshot    = CreateQuestionnaireDocument(questionnaireId, title, ownerId, isPublic: false);

            var questionnaireStorageMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireStorageMock
                .Setup(s => s.Get(It.Is<QuestionnaireRevision>(r => r.Revision == revisionId)))
                .Returns(historicalSnapshot);
            questionnaireStorageMock
                .Setup(s => s.Get(questionnaireId))
                .Returns(currentSnapshot);

            var dbContext = Create.InMemoryDbContext();
            var loggedInUser = Mock.Of<ILoggedInUser>(u => u.Id == requestingUserId && u.IsAdmin == false);

            var commandInflater = CreateCommandInflater(
                questionnaireStorage: questionnaireStorageMock.Object,
                dbContext: dbContext,
                loggedInUser: loggedInUser);

            var command = new PasteAfter(questionnaireId, entityId, entityId, questionnaireId, entityId, requestingUserId, revisionId);

            Action act = () => commandInflater.PrepareDeserializedCommandForExecution(command);

            act.Should().Throw<CommandInflaitingException>()
               .Where(e => e.ExceptionType == CommandInflatingExceptionType.Forbidden);
        }

        [Test]
        public void should_deny_access_when_questionnaire_was_owned_by_user_at_revision_but_ownership_has_changed()
        {
            // Historical snapshot has requestingUser as creator; current owner is someone else.
            var historicalSnapshot = CreateQuestionnaireDocument(questionnaireId, title, requestingUserId, isPublic: false);
            var currentSnapshot    = CreateQuestionnaireDocument(questionnaireId, title, newOwnerId,       isPublic: false);

            var questionnaireStorageMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireStorageMock
                .Setup(s => s.Get(It.Is<QuestionnaireRevision>(r => r.Revision == revisionId)))
                .Returns(historicalSnapshot);
            questionnaireStorageMock
                .Setup(s => s.Get(questionnaireId))
                .Returns(currentSnapshot);

            var dbContext = Create.InMemoryDbContext();
            var loggedInUser = Mock.Of<ILoggedInUser>(u => u.Id == requestingUserId && u.IsAdmin == false);

            var commandInflater = CreateCommandInflater(
                questionnaireStorage: questionnaireStorageMock.Object,
                dbContext: dbContext,
                loggedInUser: loggedInUser);

            var command = new PasteAfter(questionnaireId, entityId, entityId, questionnaireId, entityId, requestingUserId, revisionId);

            Action act = () => commandInflater.PrepareDeserializedCommandForExecution(command);

            act.Should().Throw<CommandInflaitingException>()
               .Where(e => e.ExceptionType == CommandInflatingExceptionType.Forbidden);
        }

        private static readonly Guid questionnaireId  = Guid.Parse("10000000000000000000000000000001");
        private static readonly Guid revisionId       = Guid.Parse("20000000000000000000000000000002");
        private static readonly Guid entityId         = Guid.Parse("30000000000000000000000000000003");
        private static readonly Guid requestingUserId = Guid.Parse("40000000000000000000000000000004");
        private static readonly Guid ownerId          = Guid.Parse("50000000000000000000000000000005");
        private static readonly Guid newOwnerId       = Guid.Parse("60000000000000000000000000000006");
        private const string title = "questionnaire title";
    }
}
