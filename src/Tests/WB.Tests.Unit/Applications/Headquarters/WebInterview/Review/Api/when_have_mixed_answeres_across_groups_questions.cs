using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_have_mixed_answeres_across_groups_questions : WebInterviewInterviewEntityFactorySpecification
    {
        [SetUp]
        public void Context()
        {
            this.AnswerTextQuestions(
                SecA, SecA_In, SecA_Roster_In, SecA_Roster_Sup, 
                SecB, SecB_In 
            );
        }
        /*                                  ForInterviewer  | ForSupervisor
            SecA:                           Completed       | Started
                [x] SecA_In
                [ ] SecA_Sup
                SecA_Roster:                Completed       | Completed
                    [x] SecA_Roster_In
                    [x] SecA_Roster_Sup
            SecB:                           Started         | Started
                [x] SecB_In
                [ ] SecB_Sup
                SecB_Group:                 NotStarted      | NotStarted
                    [ ] SecB_Group_In
                    [ ] SecB_Group_Sup
         
         */

        public static (Identity sectionId, GroupStatus expectedStatus)[] ForSupervisor =
        {
            (SecA,        GroupStatus.Started),
            (SecA_Roster, GroupStatus.Completed),

            (SecB,        GroupStatus.Started),
            (SecB_Group,  GroupStatus.NotStarted) 
        };

        public static (Identity sectionId, GroupStatus expectedStatus)[] ForInterviewer =
        {
            (SecA,        GroupStatus.Completed),
            (SecA_Roster, GroupStatus.Completed),

            (SecB,        GroupStatus.Started), 
            (SecB_Group,  GroupStatus.NotStarted)
        };

        [Test, TestCaseSource(nameof(ForSupervisor))]
        public void supervisor_should_see_group_or_roster_in_status((Identity sectionId, GroupStatus expectedStatus) testCase)
        {
            this.AsSupervisor();
            Assert.That(this.GetGroupDetails(testCase.sectionId).Status, Is.EqualTo(testCase.expectedStatus));
        }

        [Test, TestCaseSource(nameof(ForInterviewer))]
        public void interviewer_should_see_group_or_roster_in_status((Identity sectionId, GroupStatus expectedStatus) testCase)
        {
            this.AsInterviewer();
            Assert.That(this.GetGroupDetails(testCase.sectionId).Status, Is.EqualTo(testCase.expectedStatus));
        }
    }
}