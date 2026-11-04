using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(AnonymousSharingPostProcessor))]
    [TestFixture]
    public class AnonymousSharingPostProcessorTests
    {
        private static readonly Guid QuestionnaireId = Id.g1;
        private static readonly Guid ResponsibleId = Id.g2;

        [Test]
        public void When_enabling_for_the_first_time_Then_active_record_should_be_created()
        {
            var dbContext = Create.InMemoryDbContext();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new EnableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.True);
            Assert.That(record.AnonymousQuestionnaireId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void When_enabling_disabled_questionnaire_Then_existing_link_should_be_reused()
        {
            var dbContext = Create.InMemoryDbContext();
            var anonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(anonymousId, isActive: false));
            dbContext.SaveChanges();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new EnableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.True);
            Assert.That(record.AnonymousQuestionnaireId, Is.EqualTo(anonymousId));
        }

        [Test]
        public void When_enabling_already_enabled_questionnaire_Then_state_should_not_change()
        {
            var dbContext = Create.InMemoryDbContext();
            var anonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(anonymousId, isActive: true));
            dbContext.SaveChanges();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new EnableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.True);
            Assert.That(record.AnonymousQuestionnaireId, Is.EqualTo(anonymousId));
        }

        [Test]
        public void When_disabling_Then_record_should_become_inactive_and_keep_its_link()
        {
            var dbContext = Create.InMemoryDbContext();
            var anonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(anonymousId, isActive: true));
            dbContext.SaveChanges();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new DisableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.False);
            Assert.That(record.AnonymousQuestionnaireId, Is.EqualTo(anonymousId));
        }

        [Test]
        public void When_disabling_already_disabled_questionnaire_Then_state_should_not_change()
        {
            var dbContext = Create.InMemoryDbContext();
            var anonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(anonymousId, isActive: false));
            dbContext.SaveChanges();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new DisableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.False);
            Assert.That(record.AnonymousQuestionnaireId, Is.EqualTo(anonymousId));
        }

        [Test]
        public void When_disabling_questionnaire_that_was_never_shared_Then_nothing_should_be_created()
        {
            var dbContext = Create.InMemoryDbContext();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(CreateQuestionnaire(), new DisableAnonymousSharing(QuestionnaireId, ResponsibleId));

            Assert.That(dbContext.AnonymousQuestionnaires.Any(), Is.False);
        }

        [Test]
        public void When_regenerating_link_Then_new_active_record_should_replace_the_old_one()
        {
            var dbContext = Create.InMemoryDbContext();
            var oldAnonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(oldAnonymousId, isActive: true));
            dbContext.SaveChanges();
            var postProcessor = CreatePostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new RegenerateAnonymousSharingLink(QuestionnaireId, ResponsibleId, false));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.True);
            Assert.That(record.AnonymousQuestionnaireId, Is.Not.EqualTo(oldAnonymousId));
        }

        [TestCase(true, QuestionnaireActionType.AnonymousSharingEnabled)]
        [TestCase(false, QuestionnaireActionType.AnonymousSharingDisabled)]
        public void When_sharing_state_changes_Then_matching_history_item_should_be_added(
            bool enable,
            QuestionnaireActionType expectedAction)
        {
            var dbContext = Create.InMemoryDbContext();
            dbContext.Users.Add(new DesignerIdentityUser { Id = ResponsibleId, UserName = "responsible" });
            dbContext.SaveChanges();
            if (!enable)
            {
                dbContext.AnonymousQuestionnaires.Add(CreateRecord(Guid.NewGuid(), isActive: true));
                dbContext.SaveChanges();
            }

            var questionnaire = CreateQuestionnaire();
            var postProcessor = CreatePostProcessor(dbContext);
            if (enable)
                postProcessor.Process(questionnaire, new EnableAnonymousSharing(QuestionnaireId, ResponsibleId));
            else
                postProcessor.Process(questionnaire, new DisableAnonymousSharing(QuestionnaireId, ResponsibleId));

            var historyItem = dbContext.QuestionnaireChangeRecords.Single();
            Assert.That(historyItem.ActionType, Is.EqualTo(expectedAction));
            Assert.That(historyItem.TargetItemTitle, Is.EqualTo(questionnaire.QuestionnaireDocument.Title));
            Assert.That(historyItem.UserId, Is.EqualTo(ResponsibleId));
            Assert.That(historyItem.UserName, Is.EqualTo("responsible"));
        }

        private static AnonymousSharingPostProcessor CreatePostProcessor(DesignerDbContext dbContext)
            => new(dbContext, Create.QuestionnireHistoryVersionsService(dbContext));

        private static WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire CreateQuestionnaire()
            => Create.Questionnaire(ResponsibleId,
                Create.QuestionnaireDocument(QuestionnaireId, title: "Test questionnaire"));

        private static AnonymousQuestionnaire CreateRecord(Guid anonymousId, bool isActive)
            => new AnonymousQuestionnaire
            {
                QuestionnaireId = QuestionnaireId,
                AnonymousQuestionnaireId = anonymousId,
                IsActive = isActive,
                GeneratedAtUtc = DateTime.UtcNow
            };
    }
}
