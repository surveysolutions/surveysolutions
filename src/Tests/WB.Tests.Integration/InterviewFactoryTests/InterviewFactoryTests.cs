using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewTests.LinkedQuestions;

namespace WB.Tests.Integration.InterviewFactoryTests
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests : InterviewFactorySpecification
    {
        protected void StoreQuestionnaireDocument(QuestionnaireDocument document)
        {
            //new HqQuestionnaireStorage()
        }

        [Test]
        public void when_getting_flagged_question_ids()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            var questionIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1))
            };

            interviewSummaryRepository.Store(  Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                questionnaireId: questionnaireId.QuestionnaireId,
                questionnaireVersion: questionnaireId.Version,
                receivedByInterviewerAtUtc: null), interviewId.FormatGuid());

            var factory = CreateInterviewFactory();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionIdentities[0].Id),
                    Create.Entity.TextQuestion(questionIdentities[1].Id),
                });

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            foreach (var questionIdentity in questionIdentities)
                factory.SetFlagToQuestion(interviewId, questionIdentity, true);

            //act
            var flaggedIdentites = factory.GetFlaggedQuestionIds(interviewId);

            //assert
            ClassicAssert.AreEqual(2, flaggedIdentites.Length);
            Assert.That(flaggedIdentites, Is.EquivalentTo(questionIdentities));
        }

        [Test]
        public void when_setting_flags_should_get_proper_one()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            var questionIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1))
            };

            interviewSummaryRepository.Store(  Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                questionnaireId: questionnaireId.QuestionnaireId,
                questionnaireVersion: questionnaireId.Version,
                receivedByInterviewerAtUtc: null), interviewId.FormatGuid());

            var factory = CreateInterviewFactory();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionIdentities[0].Id),
                    Create.Entity.TextQuestion(questionIdentities[1].Id),
                });

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            foreach (var questionIdentity in questionIdentities)
            {
                factory.SetFlagToQuestion(interviewId, questionIdentity, true);
            }

            factory.SetFlagToQuestion(interviewId, questionIdentities[0], false);

            //act
            var flaggedIdentites = factory.GetFlaggedQuestionIds(interviewId);

            //assert
            ClassicAssert.AreEqual(1, flaggedIdentites.Length);
            Assert.That(flaggedIdentites, Is.EquivalentTo(new[] {questionIdentities[1]}));
        }

        [Test]
        public void when_remove_flag_question_received_by_interviewer()
        {
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Parse("111111111111111111111111111111111");
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            interviewSummaryRepository.Store(
                Create.Entity.InterviewSummary(
                    interviewId: interviewId,
                    status: InterviewStatus.Completed,
                    questionnaireId: questionnaireId.QuestionnaireId,
                    questionnaireVersion: questionnaireId.Version,
                    receivedByInterviewerAtUtc: DateTime.UtcNow.AddDays(-10)), interviewId.FormatGuid());

            var factory = CreateInterviewFactory();

            //act
            var exception =
                Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, false));

            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message,
                Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }

        [Test]
        public void when_set_flag_question_received_by_interviewer()
        {
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Parse("111111111111111111111111111111111");
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            interviewSummaryRepository.Store(Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                questionnaireId: questionnaireId.QuestionnaireId,
                questionnaireVersion: questionnaireId.Version,
                receivedByInterviewerAtUtc: DateTime.UtcNow.AddDays(-10)), interviewId.FormatGuid());

            var factory = CreateInterviewFactory();

            //act
            var exception =
                Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, true));

            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message,
                Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }
    }
}
