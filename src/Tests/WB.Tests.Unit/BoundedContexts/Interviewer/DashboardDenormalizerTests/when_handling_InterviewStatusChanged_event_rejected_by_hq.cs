using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    public class when_handling_InterviewStatusChanged_event_rejected_by_hq
    {
        [Test]
        public  void should_interview_be_in_rejected_status_for_supervisor()
        {
            var interviewId = Guid.Parse("22222222222222222222222222222222");

            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("33333333333333333333333333333333"), 1);
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var eventTime = DateTime.UtcNow;

            var @event = Create.Event.InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, utcTime: eventTime)
                .ToPublishedEvent(interviewId);

            var interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString()));

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaire: Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocument(questionnaireIdentity.QuestionnaireId, new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: questionId, scope: QuestionScope.Supervisor)
                    })));

            var denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);

            // Act
            denormalizer.Handle(@event);

            // Assert
            var interviewView = interviewViewStorage.GetById(interviewId.FormatGuid());

            Assert.That(interviewView.RejectedDateTime, Is.EqualTo(eventTime));
            Assert.That(interviewView.Status, Is.EqualTo(InterviewStatus.RejectedByHeadquarters));
        }
    }
}
