using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    [TestFixture]
    internal class when_handling_AnswersRemoved_event_for_prefilled_GPS_question
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            var gpsQuestionId = Guid.Parse("11111111111111111111111111111111");

            dashboardItem = Create.Entity.InterviewView();
            dashboardItem.QuestionnaireId = questionnaireIdentity.ToString();
            dashboardItem.LocationQuestionId = gpsQuestionId;
            dashboardItem.LocationLongitude = 10;
            dashboardItem.LocationLatitude = 20;

            @event = Create.Event.AnswersRemoved(Create.Entity.Identity("11111111111111111111111111111111", RosterVector.Empty)).ToPublishedEvent();

            var interviewViewStorage = Mock.Of<IPlainStorage<InterviewView>>(writer =>
            writer.GetById(It.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.Store(It.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view);

            var questionnaire = Mock.Of<IQuestionnaire>(q => q.IsPrefilled(gpsQuestionId) == true);
            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(r =>
                r.GetQuestionnaire(questionnaireIdentity, Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);

            denormalizer.Handle(@event);
        }

        [Test]
        public void should_clear_GPS_location_latitude() =>
            Assert.That(dashboardItem.LocationLatitude, Is.Null);

        [Test]
        public void should_clear_GPS_location_longitude() =>
            Assert.That(dashboardItem.LocationLongitude, Is.Null);

        static InterviewDashboardEventHandler denormalizer;
        static IPublishedEvent<AnswersRemoved> @event;
        static InterviewView dashboardItem;
    }
}
