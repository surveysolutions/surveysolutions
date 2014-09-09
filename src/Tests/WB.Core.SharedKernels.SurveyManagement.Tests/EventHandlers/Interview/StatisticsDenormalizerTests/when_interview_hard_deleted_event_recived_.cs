using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Utility;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.StatisticsDenormalizerTests
{
    internal class when_interview_hard_deleted_event_recived : StatisticsDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();
            statisticsStorage = new TestInMemoryWriter<StatisticsLineGroupedByUserAndTemplate>();
            interviewBriefStorage = new TestInMemoryWriter<InterviewBrief>();

            var interviewBriefItem = new InterviewBrief() { Status = InterviewStatus.InterviewerAssigned };
            statisticsLineGroupedByUserAndTemplate = new StatisticsLineGroupedByUserAndTemplate()
            {
                TotalCount = 1,
                InterviewerAssignedCount = 1
            };

            interviewBriefStorage.Store(interviewBriefItem, interviewId.FormatGuid());

            statisticsStorage.Store(statisticsLineGroupedByUserAndTemplate, interviewBriefItem.QuestionnaireId
                .Combine(interviewBriefItem.QuestionnaireVersion)
                .Combine(interviewBriefItem.ResponsibleId).FormatGuid());
            denormalizer = CreateStatisticsDenormalizer(statisticsStorage: statisticsStorage,
                interviewBriefStorage: interviewBriefStorage);

            interviewHardDeletedEvent = ToPublishedEvent(new InterviewHardDeleted(Guid.NewGuid()), interviewId);
        };

        Because of = () => denormalizer.Handle(interviewHardDeletedEvent);

        It should_interview_view_be_deleted_from_storage = () => interviewBriefStorage.GetById(interviewId.FormatGuid()).ShouldBeNull();

        It should_statisticsLineGroupedByUserAndTemplate_has_zero_interview_in_total = () => statisticsLineGroupedByUserAndTemplate.TotalCount.ShouldEqual(0);

        It should_statisticsLineGroupedByUserAndTemplate_has_zero_interview_in_InterviewerAssignedCount = () => statisticsLineGroupedByUserAndTemplate.InterviewerAssignedCount.ShouldEqual(0);

        private static StatisticsDenormalizer denormalizer;
        private static TestInMemoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage;
        private static IPublishedEvent<InterviewHardDeleted> interviewHardDeletedEvent;
        private static TestInMemoryWriter<InterviewBrief> interviewBriefStorage;
        private static StatisticsLineGroupedByUserAndTemplate statisticsLineGroupedByUserAndTemplate;
        private static Guid interviewId;
    }
}
