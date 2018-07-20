using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_applying_synchronization_metadata_on_created_interviews : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionnaire);
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() 
        {
            foreach (var originalInterviewStatus in Enum.GetValues(typeof (InterviewStatus)).Cast<InterviewStatus>())
            {
                var exceptionByStatuses = new List<InterviewStatus>();
                foreach (var interviewStatus in Enum.GetValues(typeof (InterviewStatus)).Cast<InterviewStatus>())
                {
                    var interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
                    interview.Apply(new InterviewStatusChanged(originalInterviewStatus, "", DateTimeOffset.Now));
                    try
                    {
                        interview.CreateInterviewFromSynchronizationMetadata(interview.EventSourceId, userId, questionnaireId,1, interviewStatus, null, "", null, null,
                            false, false, DateTimeOffset.Now);
                        exceptionByStatuses.Add(interviewStatus);
                    }
                    catch {}
                }
                if (exceptionByStatuses.Any())
                    interviewStatusesWhichWasChangedWithoutException.Add(originalInterviewStatus, exceptionByStatuses.ToArray());
            }
        }

        [NUnit.Framework.Test] public void should_count_of_interview_statuses_succefully_updated_be_equal_to_9 () =>
            interviewStatusesWhichWasChangedWithoutException.Keys.Count().Should().Be(9);

        [NUnit.Framework.Test] public void should_interview_in_status_Deleted_be_allowed_to_change_on_any_status () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Deleted].Length.Should().Be(
                Enum.GetValues(typeof (InterviewStatus)).Length);

        [NUnit.Framework.Test] public void should_interview_in_status_Restored_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Restored].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor
            });

        [NUnit.Framework.Test] public void should_interview_in_status_SupervisorAssigned_be_allowed_to_change_on_ApprovedBySupervisor_InterviewerAssigned_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.SupervisorAssigned].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.ApprovedBySupervisor
            });

        [NUnit.Framework.Test] public void should_interview_in_status_InterviewerAssigned_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_ApprovedBySupervisor_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.InterviewerAssigned].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor, InterviewStatus.ApprovedBySupervisor
            });

        [NUnit.Framework.Test] public void should_interview_in_status_Completed_be_allowed_to_change_on_InterviewerAssigned_RejectedBySupervisor_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Completed].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor
            });

        [NUnit.Framework.Test] public void should_interview_in_status_Restarted_be_allowed_to_change_on_Completed_RejectedBySupervisor_InterviewerAssigned_recheck_this_one () =>
             interviewStatusesWhichWasChangedWithoutException[InterviewStatus.Restarted].Should().BeEquivalentTo(new[]
             {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor
             });

        [NUnit.Framework.Test] public void should_interview_in_status_RejectedBySupervisor_be_allowed_to_change_on_RejectedBySupervisor_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.RejectedBySupervisor].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor, InterviewStatus.ApprovedBySupervisor, 
            });

        [NUnit.Framework.Test] public void should_interview_in_status_ApprovedBySupervisor_be_allowed_to_change_on_RejectedBySupervisor_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.ApprovedBySupervisor].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.RejectedBySupervisor
            });

        [NUnit.Framework.Test] public void should_interview_in_status_RejectedByHeadquarters_be_allowed_to_change_on_ApprovedBySupervisor_recheck_this_one () =>
            interviewStatusesWhichWasChangedWithoutException[InterviewStatus.RejectedByHeadquarters].Should().BeEquivalentTo(new[]
            {
                InterviewStatus.ApprovedBySupervisor
            });

        private static EventContext eventContext;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static long questionnaireVersion = 18;

        private static Dictionary<InterviewStatus, InterviewStatus[]> interviewStatusesWhichWasChangedWithoutException =
            new Dictionary<InterviewStatus, InterviewStatus[]>();

        private static IQuestionnaireStorage questionnaireRepository;
    }
}
