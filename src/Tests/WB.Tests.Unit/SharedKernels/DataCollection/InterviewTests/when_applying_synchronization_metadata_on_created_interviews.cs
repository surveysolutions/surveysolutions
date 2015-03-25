using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_applying_synchronization_metadata_on_created_interviews : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionnaire &&
                    repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
        {
            foreach (var originalInterviewStatus in Enum.GetValues(typeof (InterviewStatus)).Cast<InterviewStatus>())
            {
                var exceptionByStatuses = new List<InterviewStatus>();
                foreach (var interviewStatus in Enum.GetValues(typeof (InterviewStatus)).Cast<InterviewStatus>())
                {
                    var interview = new Interview();
                    interview.Apply(new InterviewStatusChanged(originalInterviewStatus, ""));
                    try
                    {
                        interview.ApplySynchronizationMetadata(interview.EventSourceId, userId, questionnaireId,1, interviewStatus, null, "",
                            false, false);
                        exceptionByStatuses.Add(interviewStatus);
                    }
                    catch {}
                }
                if (exceptionByStatuses.Any())
                    interviewStatusesWhichWasChangedWithoutException.Add(originalInterviewStatus, exceptionByStatuses.ToArray());
            }
        };

        It should_count_of_interview_statuses_succefully_updated_be_equal_to_9 = () =>
            interviewStatusesWhichWasChangedWithoutException.Keys.Count().ShouldEqual(9);

        It should_interview_in_status_Deleted_be_allowed_to_change_on_any_status = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Deleted].Length.ShouldEqual(
                Enum.GetValues(typeof (InterviewStatus)).Length);

        It should_interview_in_status_Restored_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Restored].ShouldContainOnly(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor
            });

        It should_interview_in_status_SupervisorAssigned_be_allowed_to_change_on_ApprovedBySupervisor_InterviewerAssigned_recheck_this_one  = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.SupervisorAssigned].ShouldContainOnly(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.ApprovedBySupervisor
            });

        It should_interview_in_status_InterviewerAssigned_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_ApprovedBySupervisor_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.InterviewerAssigned].ShouldContainOnly(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor, InterviewStatus.ApprovedBySupervisor
            });

        It should_interview_in_status_Completed_be_allowed_to_change_on_RejectedBySupervisor_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Completed].ShouldContainOnly(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor
            });

        It should_interview_in_status_Restarted_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_recheck_this_one = () =>
             interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Restarted].ShouldContainOnly(new[]
             {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor
             });

        It should_interview_in_status_RejectedBySupervisor_be_allowed_to_change_on_RejectedBySupervisor_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.RejectedBySupervisor].ShouldContainOnly(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor
            });

        It should_interview_in_status_ApprovedBySupervisor_be_allowed_to_change_on_RejectedBySupervisor_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.ApprovedBySupervisor].ShouldContainOnly(new[]
            {
                InterviewStatus.RejectedBySupervisor
            });

        It should_interview_in_status_RejectedByHeadquarters_be_allowed_to_change_on_ApprovedBySupervisor_recheck_this_one = () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.RejectedByHeadquarters].ShouldContainOnly(new[]
            {
                InterviewStatus.ApprovedBySupervisor
            });

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Dictionary<InterviewStatus, InterviewStatus[]> interviewStatusesWhichWasChangedWithoutException =
            new Dictionary<InterviewStatus, InterviewStatus[]>();
    }
}
