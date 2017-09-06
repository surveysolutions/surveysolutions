using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.CumulativeChartDenormalizerTests
{
    internal class when_handling_InterviewStatusChanged_event_and_there_are_no_last_status_for_this_interview
    {
        Establish context = () =>
        {
            @event = Create.PublishedEvent.InterviewStatusChanged(
                interviewId: Guid.Parse(interviewStringId),
                status: newStatus);

            var lastStatusesStorage = Mock.Of<IReadSideKeyValueStorage<LastInterviewStatus>>(_ =>
                _.GetById(interviewStringId) == null as LastInterviewStatus);

            var interviewReferences = Create.Entity.InterviewSummary(@event.EventSourceId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>("1", interviewReferences);
            
            denormalizer = Create.Service.CumulativeChartDenormalizer(
                lastStatusesStorage: lastStatusesStorage,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeStorageMock.Object,
                interviewReferencesStorage: interviewReferencesStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_not_store_interview_change_with_questionnaire_and_event_date_and_last_status_and_minus_one_value = () =>
            cumulativeReportStatusChangeStorageMock.Verify(storage =>
                storage.Store(
                    Moq.It.Is<CumulativeReportStatusChange>(change =>
                        change.QuestionnaireId == questionnaireId &&
                        change.QuestionnaireVersion == questionnaireVersion &&
                        change.Date == @event.EventTimeStamp.Date &&
                        change.Status == lastStatus &&
                        change.ChangeValue == -1),
                    Moq.It.IsAny<string>()),
                Times.Never);

        It should_store_interview_change_with_questionnaire_and_event_date_and_new_status_and_plus_one_value = () =>
            cumulativeReportStatusChangeStorageMock.Verify(storage =>
                storage.Store(
                    Moq.It.Is<CumulativeReportStatusChange>(change =>
                        change.QuestionnaireId == questionnaireId &&
                        change.QuestionnaireVersion == questionnaireVersion &&
                        change.Date == @event.EventTimeStamp.Date &&
                        change.Status == newStatus &&
                        change.ChangeValue == +1),
                    Moq.It.IsAny<string>()),
                Times.Once);

        private static CumulativeChartDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> @event;
        private static Mock<IReadSideRepositoryWriter<CumulativeReportStatusChange>> cumulativeReportStatusChangeStorageMock = new Mock<IReadSideRepositoryWriter<CumulativeReportStatusChange>>();
        private static string interviewStringId = "11111111111111111111111111111111";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static InterviewStatus newStatus = InterviewStatus.ApprovedBySupervisor;
        private static Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static long questionnaireVersion = 7112;
    }
}