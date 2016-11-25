using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.Interview
{
    [TestFixture]
    internal class AllInterviewsFactoryTests
    {
        [Test]
        public void Load_When_load_interviews_with_differant_statuses_Then_CanBeReassigned_flag_should_set_correctly()
        {
            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedByHeadquarters), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.SupervisorAssigned), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedBySupervisor), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.Completed), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.Created), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.Deleted), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.ReadyForInterview), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.RejectedByHeadquarters), Guid.NewGuid());
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor), Guid.NewGuid());
            var featuredQuestionsStorage = Create.Storage.InMemoryReadeSideStorage<QuestionAnswer>();
            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage, featuredQuestionsStorage);

            var interviews = interviewsFactory.Load(new AllInterviewsInputModel());

            var items = interviews.Items.ToList();
            Assert.That(items[0].CanBeReassigned, Is.False);
            Assert.That(items[1].CanBeReassigned, Is.True); // SupervisorAssigned
            Assert.That(items[2].CanBeReassigned, Is.False);
            Assert.That(items[3].CanBeReassigned, Is.True); // Completed
            Assert.That(items[4].CanBeReassigned, Is.False);
            Assert.That(items[5].CanBeReassigned, Is.False);
            Assert.That(items[6].CanBeReassigned, Is.True); // InterviewerAssigned
            Assert.That(items[7].CanBeReassigned, Is.False);
            Assert.That(items[8].CanBeReassigned, Is.True); // RejectedByHeadquarters
            Assert.That(items[9].CanBeReassigned, Is.True); // RejectedBySupervisor
        }
    }
}