using NUnit.Framework;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_all_answers_valid_and_answered : WebInterviewInterviewEntityFactorySpecification
    {
        [SetUp]
        public void Context()
        {
            this.AnswerTextQuestions(
                SecA_In, SecA_Sup, SecA_Roster_In, SecA_Roster_Sup, // Section A answered
                SecB_In, SecB_Sup, SecB_Group_In, SecB_Group_Sup    // Section B answered
            );
        }

        [Test]
        public void should_mark_allGroups_as_completed_for_interviewer()
        {
            this.AsInterviewer();
            Assert.That(this.GetGroupDetails(SecA).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecB).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecA_Roster).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecB_Group).Status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void should_mark_all_groups_as_valid_for_interviewer()
        {
            this.AsInterviewer();
            Assert.That(this.GetGroupDetails(SecA).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecB).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecA_Roster).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecB_Group).Validity.IsValid, Is.True);
        }

        [Test]
        public void should_mark_all_groups_as_valid_for_supervisor()
        {
            this.AsSupervisor();

            Assert.That(this.GetGroupDetails(SecA).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecB).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecA_Roster).Validity.IsValid, Is.True);
            Assert.That(this.GetGroupDetails(SecB_Group).Validity.IsValid, Is.True);
        }

        [Test]
        public void should_mark_allGroups_as_completed_for_supervisor()
        {
            this.AsSupervisor();

            Assert.That(this.GetGroupDetails(SecA).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecB).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecA_Roster).Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(this.GetGroupDetails(SecB_Group).Status, Is.EqualTo(GroupStatus.Completed));
        }
    }
}