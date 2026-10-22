using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
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
            var postProcessor = new AnonymousSharingPostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new EnableAnonymousSharing(QuestionnaireId, ResponsibleId, false));

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
            var postProcessor = new AnonymousSharingPostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new EnableAnonymousSharing(QuestionnaireId, ResponsibleId, false));

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
            var postProcessor = new AnonymousSharingPostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new DisableAnonymousSharing(QuestionnaireId, ResponsibleId, false));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.False);
            Assert.That(record.AnonymousQuestionnaireId, Is.EqualTo(anonymousId));
        }

        [Test]
        public void When_disabling_questionnaire_that_was_never_shared_Then_nothing_should_be_created()
        {
            var dbContext = Create.InMemoryDbContext();
            var postProcessor = new AnonymousSharingPostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new DisableAnonymousSharing(QuestionnaireId, ResponsibleId, false));

            Assert.That(dbContext.AnonymousQuestionnaires.Any(), Is.False);
        }

        [Test]
        public void When_regenerating_link_Then_new_active_record_should_replace_the_old_one()
        {
            var dbContext = Create.InMemoryDbContext();
            var oldAnonymousId = Guid.NewGuid();
            dbContext.AnonymousQuestionnaires.Add(CreateRecord(oldAnonymousId, isActive: true));
            dbContext.SaveChanges();
            var postProcessor = new AnonymousSharingPostProcessor(dbContext);

            postProcessor.Process(Create.Questionnaire(), new RegenerateAnonymousSharingLink(QuestionnaireId, ResponsibleId, false));

            var record = dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == QuestionnaireId);
            Assert.That(record.IsActive, Is.True);
            Assert.That(record.AnonymousQuestionnaireId, Is.Not.EqualTo(oldAnonymousId));
        }

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

