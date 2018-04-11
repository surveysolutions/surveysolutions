using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.CumulativeChartDenormalizerTests
{
    [TestFixture]
    internal class when_handling_events_list_having_several_InterviewStatusChanged_events
    {
        [SetUp]
        public void Establish()
        {
            cumulativeReportStatusChangeStorage = new TestInMemoryWriter<CumulativeReportStatusChange>();
            @events.Add(Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId, status: firstStatus, eventId: Id.g7));
            @events.Add(Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId, previousStatus: firstStatus, status: secondStatus, eventId: Id.g9));

            var interviewReferences = Create.Entity.InterviewSummary(interviewId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>(interviewId.FormatGuid(), interviewReferences);
            var cumulativeReportReader = new TestInMemoryWriter<CumulativeReportStatusChange>();

            denormalizer = Create.Service.CumulativeChartDenormalizer(
                cumulativeReportReader: cumulativeReportReader,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeStorage,
                interviewReferencesStorage: interviewReferencesStorage);
        }

        [Test]
        public void should_store_interview_change_with_questionnaire_and_event_date_and_new_status_and_plus_one_value()
        {
            denormalizer.Handle(@events, interviewId);

            var superPlusKey = $"{@Id.g7.FormatGuid()}-plus";
            var superPlusRecord = cumulativeReportStatusChangeStorage.GetById(superPlusKey);

            var superMinusKey = $"{@Id.g9.FormatGuid()}-minus";
            var superMinusRecord = cumulativeReportStatusChangeStorage.GetById(superMinusKey);

            var plusKey = $"{@Id.g9.FormatGuid()}-plus";
            var plusRecord = cumulativeReportStatusChangeStorage.GetById(plusKey);

            Assert.That(superPlusRecord, Is.Not.Null);
            Assert.That(superPlusRecord.QuestionnaireIdentity, Is.EqualTo(questionnaireIdentity));

            Assert.That(superMinusRecord, Is.Not.Null);
            Assert.That(superMinusRecord.QuestionnaireIdentity, Is.EqualTo(questionnaireIdentity));

            Assert.That(plusRecord, Is.Not.Null);
            Assert.That(plusRecord.QuestionnaireIdentity, Is.EqualTo(questionnaireIdentity));
        }

        private static CumulativeChartDenormalizer denormalizer;
        private static List<IPublishedEvent<InterviewStatusChanged>> @events = new List<IPublishedEvent<InterviewStatusChanged>>();
        private static TestInMemoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;

        private static InterviewStatus firstStatus = InterviewStatus.SupervisorAssigned;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewStatus secondStatus = InterviewStatus.InterviewerAssigned;
        private static readonly Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string questionnaireIdentity = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$7112";
        private static long questionnaireVersion = 7112;
    }
}
