using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_TextListQuestionAnswered_event
    {
        [OneTimeSetUp]
        public void context()
        {
            evnt = Create.Event.TextListQuestionAnswered(questionId, originDate: this.answerTime).ToPublishedEvent(interviewId);

            interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId, questionnaireId: questionnaireIdentity.ToString()));

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId,  children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(questionId: questionId)
                    })));

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);
        }

        [SetUp]
        public void because_of() =>
            denormalizer.Handle(evnt);

        [Test]
        public void should_set_answer_time_as_start_date_for_interview() =>
            interviewViewStorage.GetById(interviewId.FormatGuid())?.StartedDateTime.Should().Be(answerTime.UtcDateTime);

        private static InterviewDashboardEventHandler denormalizer;
        private static IPublishedEvent<TextListQuestionAnswered> evnt;
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static SqliteInmemoryStorage<InterviewView> interviewViewStorage;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private readonly DateTimeOffset answerTime = new DateTimeOffset (new DateTime(2000, 3, 28).ToUniversalTime());
    }
}
