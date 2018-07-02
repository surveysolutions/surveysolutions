using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_YesNoQuestionAnswered_event
    {
        [OneTimeSetUp]
        public void context()
        {
            interviewId = Guid.Parse("22222222222222222222222222222222");

            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
            var questionId = Guid.Parse("11111111111111111111111111111111");

            @event = Create.Event.YesNoQuestionAnswered(questionId, new AnsweredYesNoOption[0]).ToPublishedEvent(interviewId);

            interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString()));

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId, new IComposite[]
                    {
                        Create.Entity.YesNoQuestion(questionId: questionId)
                    })));

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            denormalizer.Handle(@event);
        
        [Test]
        public void should_interview_be_strated() =>
            interviewViewStorage.GetById(interviewId.FormatGuid())?.StartedDateTime.Should().NotBeNull();

        private static InterviewDashboardEventHandler denormalizer;
        private static IPublishedEvent<YesNoQuestionAnswered> @event;
        private static Guid interviewId;
        private static SqliteInmemoryStorage<InterviewView> interviewViewStorage;
    }
}
