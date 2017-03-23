using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;

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

        [TestCase]
        public void When_loading_interviews_without_prefilled_questions()
        {
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            string key = "11-11-11-11";
            DateTime updateDate = new DateTime(2017, 3, 23);

            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 1, responsibleId: responsibleId, key: key, updateDate: updateDate, wasCreatedOnClient: true), Guid.NewGuid());
            // - SearchBy
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 1, responsibleId: responsibleId, key: "22-22-22-22", updateDate: updateDate, wasCreatedOnClient: true), Guid.NewGuid());
            // - CensusOnly
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 1, responsibleId: responsibleId, key: "11-11-11-12", updateDate: updateDate, wasCreatedOnClient: false), Guid.NewGuid());
            // - QuestionnaireId
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 2, responsibleId: responsibleId, key: "11-11-11-13", updateDate: updateDate, wasCreatedOnClient: true), Guid.NewGuid());
            // - ChangedFrom, ChangedTo
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 1, responsibleId: responsibleId, key: "11-11-11-14", updateDate: updateDate.AddMonths(1), wasCreatedOnClient: true), Guid.NewGuid());
            // - InterviewerId
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(questionnaireId: questionnaireId, questionnaireVersion: 1, responsibleId: Guid.Parse("11111111111111111111111111111111"), key: "11-11-11-15", updateDate: updateDate, wasCreatedOnClient: true), Guid.NewGuid());

            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage);

            var interviews = interviewsFactory.LoadInterviewsWithoutPrefilled(new InterviewsWithoutPrefilledInputModel
            {
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(questionnaireId, 1),
                ChangedFrom = new DateTime(2017, 3, 22),
                ChangedTo = new DateTime(2017, 3, 24),
                InterviewerId = responsibleId,
                CensusOnly = true,
                SearchBy = "1"
            });


            Assert.AreEqual(1, interviews.TotalCount);
        }
    }
}