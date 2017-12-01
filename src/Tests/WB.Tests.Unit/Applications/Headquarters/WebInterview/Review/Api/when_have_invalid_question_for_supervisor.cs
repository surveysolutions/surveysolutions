using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_have_invalid_question_for_supervisor : WebInterviewInterviewEntityFactorySpecification
    {
        [SetUp]
        public void Context()
        {
            this.AnswerTextQuestions(
                SecA, SecA_In, SecA_Roster_In, SecA_Roster_Sup,
                SecB, SecB_In    
            );

            this.MarkQuestionAsInvalid(SecA_Roster_Sup);
        }

        /*
                                            ForInterviewer  | ForSupervisor
            SecA:                           Completed       | Started, Invalid
                [x] SecA_In
                [ ] SecA_Sup
                SecA_Roster:                Completed       | Completed, Invalid
                    [x] SecA_Roster_In
                    [!] SecA_Roster_Sup
            SecB:                           Started         | Started
                [x] SecB_In
                [ ] SecB_Sup
                SecB_Group:                 NotStarted      | NotStarted
                    [ ] SecB_Group_In
                    [ ] SecB_Group_Sup
         
         */

        public static (Identity sectionId, GroupStatus expectedStatus, bool isValid)[] ForSupervisor =
        {
            (SecA,        GroupStatus.Started, true),
            (SecA_Roster, GroupStatus.Completed, false),

            (SecB,        GroupStatus.Started, true),
            (SecB_Group,  GroupStatus.NotStarted, true) 
        };

        public static (Identity sectionId, GroupStatus expectedStatus, bool isValid)[] ForInterviewer =
        {
            (SecA,        GroupStatus.Completed, true),
            (SecA_Roster, GroupStatus.Completed, true),

            (SecB,        GroupStatus.Started, true), 
            (SecB_Group,  GroupStatus.NotStarted, true)
        };

        private SidebarPanel GetSidebarState(Identity id)
        {
            var item = this.interview.GetGroup(id);

            var sectionIds = item.Parent == null ? new string[] {null} : new[] {item.Parent.Identity.ToString()};

            var panel = this.Subject.GetSidebarChildSectionsOf(
                id.ToString(), this.interview, sectionIds, IsReviewMode);

            return panel.Groups.SingleOrDefault(g => g.Id == id.ToString());
        }

        [Test, TestCaseSource(nameof(ForSupervisor))]
        public void supervisor_should_see_sidebar_with_proper_status_and_validity((Identity sectionId, GroupStatus expectedStatus, bool isValid) testCase)
        {
            this.AsSupervisor(); 

            var sidebarPanel = GetSidebarState(testCase.sectionId);
            
            Assert.That(sidebarPanel.Status, Is.EqualTo(testCase.expectedStatus));
            Assert.That(sidebarPanel.Validity.IsValid, Is.EqualTo(testCase.isValid), "Group {0} should has validty: {1}", testCase.sectionId, testCase.isValid);
        }

        [Test, TestCaseSource(nameof(ForSupervisor))]
        public void supervisor_should_see_group_or_roster_in_status((Identity sectionId, GroupStatus expectedStatus, bool isValid) testCase)
        {
            this.AsSupervisor();
            var groupDetails = this.GetGroupDetails(testCase.sectionId);
            
            Assert.That(groupDetails.Status, Is.EqualTo(testCase.expectedStatus));
            Assert.That(groupDetails.Validity.IsValid, Is.EqualTo(testCase.isValid), "Group {0} should has validty: {1}", testCase.sectionId, testCase.isValid);
        }

        [Test, TestCaseSource(nameof(ForInterviewer))]
        public void interviewer_should_see_group_or_roster_in_status((Identity sectionId, GroupStatus expectedStatus, bool isValid) testCase)
        {
            this.AsInterviewer();
            var groupDetails = this.GetGroupDetails(testCase.sectionId);

            Assert.That(groupDetails.Status, Is.EqualTo(testCase.expectedStatus));
            Assert.That(groupDetails.Validity.IsValid, Is.EqualTo(testCase.isValid), "Group {0} should has validty: {1}", testCase.sectionId, testCase.isValid);
        }
    }
}