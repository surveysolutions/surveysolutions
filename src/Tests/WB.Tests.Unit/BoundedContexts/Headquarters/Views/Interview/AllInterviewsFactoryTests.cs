using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.Interview
{
    [TestFixture]
    internal class AllInterviewsFactoryTests
    {
        [TestCase(InterviewStatus.SupervisorAssigned,     true)]
        [TestCase(InterviewStatus.Completed,              true)]
        [TestCase(InterviewStatus.InterviewerAssigned,    true)]
        [TestCase(InterviewStatus.RejectedByHeadquarters, true)]
        [TestCase(InterviewStatus.RejectedBySupervisor,   true)]

        [TestCase(InterviewStatus.ApprovedByHeadquarters, false)]
        [TestCase(InterviewStatus.ApprovedBySupervisor,   false)]
        [TestCase(InterviewStatus.Created,                false)]
        [TestCase(InterviewStatus.Deleted,                false)]
        [TestCase(InterviewStatus.ReadyForInterview,      false)]
        public void Load_When_load_interviews_with_statuse_Then_CanBeReassigned_flag_should_set_correctly(InterviewStatus interviewStatus, bool isAllowedReassign)
        {
            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: interviewStatus), Guid.NewGuid());
            var featuredQuestionsStorage = Create.Storage.InMemoryReadeSideStorage<QuestionAnswer>();
            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage, featuredQuestionsStorage);

            var interviews = interviewsFactory.Load(new AllInterviewsInputModel());

            var item = interviews.Items.Single();
            IResolveConstraint resolveConstraint = isAllowedReassign ? Is.True : (IResolveConstraint)Is.False;
            Assert.That(item.CanBeReassigned, resolveConstraint);
        }
    }
}