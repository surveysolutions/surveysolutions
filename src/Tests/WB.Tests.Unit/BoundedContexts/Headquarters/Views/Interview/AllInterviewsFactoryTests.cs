using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.Interview
{
    [TestFixture]
    [TestOf(typeof(AllInterviewsFactory))]
    internal class AllInterviewsFactoryTests
    {
        [TestCase(true ,InterviewStatus.SupervisorAssigned)]
        [TestCase(true ,InterviewStatus.Completed)]
        
        [TestCase(true ,InterviewStatus.InterviewerAssigned)]
        [TestCase(true ,InterviewStatus.RejectedByHeadquarters)]
        [TestCase(true ,InterviewStatus.RejectedBySupervisor  )]

        [TestCase(true, InterviewStatus.Completed, InterviewStatus.SupervisorAssigned)]
        [TestCase(true, InterviewStatus.RejectedByHeadquarters, InterviewStatus.RejectedBySupervisor)]
        [TestCase(false, InterviewStatus.ApprovedByHeadquarters, InterviewStatus.ApprovedBySupervisor)]

        [TestCase(false,InterviewStatus.ApprovedByHeadquarters)]
        [TestCase(false,InterviewStatus.ApprovedBySupervisor)]
        [TestCase(false,InterviewStatus.Created)]
        [TestCase(false,InterviewStatus.Deleted )]
        [TestCase(false, InterviewStatus.ReadyForInterview )]
        public void Load_When_load_interviews_with_statuse_Then_CanBeReassigned_flag_should_set_correctly(bool isAllowedReassign, params InterviewStatus[] interviewStatuses)
        {
            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();

            foreach (var interviewStatus in interviewStatuses)
            {
                interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: interviewStatus), Guid.NewGuid());
            }
            
            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage);

            var interviews = interviewsFactory.Load(new AllInterviewsInputModel());

            IResolveConstraint resolveConstraint = isAllowedReassign ? Is.True : (IResolveConstraint)Is.False;

            foreach (var item in interviews.Items)
            {
                Assert.That(item.CanBeReassigned, resolveConstraint);
            }
        }

        [Test]
        public void when_interview_received_by_interviewer_should_not_allow_to_delete_it()
        {
            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: true), Id.g1);

            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage);

            var interviews = interviewsFactory.Load(new AllInterviewsInputModel());

            var item = interviews.Items.First();

            Assert.That(item, Has.Property(nameof(item.CanDelete)).EqualTo(false));
        }

        [Test]
        public void when_interview_assigned_to_supervisor_and_was_completed_should_not_allow_to_delete_it()
        {
            var interviewSummaryStorage = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryStorage.Store(Create.Entity.InterviewSummary(status: InterviewStatus.SupervisorAssigned, wasCompleted: true), Id.g1);

            AllInterviewsFactory interviewsFactory = Create.Service.AllInterviewsFactory(interviewSummaryStorage);

            var interviews = interviewsFactory.Load(new AllInterviewsInputModel());

            var item = interviews.Items.First();

            Assert.That(item, Has.Property(nameof(item.CanDelete)).EqualTo(false));
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

            Assert.That(interviews.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_find_interviews_by_assignment()
        {
            Guid interviewIdWithAssignment = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid interviewIdWithoutAssignment = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var summaryThatHasAssignmentId = Create.Entity.InterviewSummary(interviewIdWithAssignment, assignmentId: 5);
            var summaryWithoutAssignment = Create.Entity.InterviewSummary(interviewIdWithoutAssignment, assignmentId: null);

            var assignments = new TestInMemoryWriter<InterviewSummary>();
            assignments.Store(summaryThatHasAssignmentId, interviewIdWithAssignment.FormatGuid());
            assignments.Store(summaryWithoutAssignment, interviewIdWithoutAssignment.FormatGuid());

            var factory = Create.Service.AllInterviewsFactory(assignments);

            // Act
            var foundEntries = factory.Load(new AllInterviewsInputModel { AssignmentId = 5 });

            // Assert
            Assert.That(foundEntries.TotalCount, Is.EqualTo(1));
            Assert.That(foundEntries.Items.Single().InterviewId, Is.EqualTo(interviewIdWithAssignment));
        }

        [Test]
        public void Should_find_interviews_by_responsibleId()
        {
            Guid interviewIdWithAssignment = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid interviewIdWithoutAssignment = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var summaryThatHasAssignmentId = Create.Entity.InterviewSummary(interviewIdWithAssignment, assignmentId: 5, responsibleId: Id.g1);
            var summaryWithoutAssignment = Create.Entity.InterviewSummary(interviewIdWithoutAssignment, assignmentId: null, responsibleId: Id.g2);

            var assignments = new TestInMemoryWriter<InterviewSummary>();
            assignments.Store(summaryThatHasAssignmentId, interviewIdWithAssignment.FormatGuid());
            assignments.Store(summaryWithoutAssignment, interviewIdWithoutAssignment.FormatGuid());

            var factory = Create.Service.AllInterviewsFactory(assignments);

            // Act
            var foundEntries = factory.Load(new AllInterviewsInputModel { ResponsibleId = Id.g1});

            // Assert
            Assert.That(foundEntries.TotalCount, Is.EqualTo(1));
            Assert.That(foundEntries.Items.Single().InterviewId, Is.EqualTo(interviewIdWithAssignment));
        }
    }
}