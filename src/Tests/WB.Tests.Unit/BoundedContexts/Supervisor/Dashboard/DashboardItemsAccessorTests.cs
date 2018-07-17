using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Dashboard
{
    [TestOf(typeof(DashboardItemsAccessor))]
    public class DashboardItemsAccessorTests
    {
        [Test]
        public void should_show_interviews_and_assignments_assigned_to_supervisor_in_waiting_for_decision_list()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();

            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Id.gA));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gA, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: Id.gB, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.Completed));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g3, responsibleId: Id.gA, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.InterviewerAssigned));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g4, responsibleId: Id.gA, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedByHeadquarters));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g9, responsibleId: Id.gA, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.ApprovedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g10, responsibleId: Id.gB, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedBySupervisor));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            IEnumerable<IDashboardItem> waitingForSupervisorAction = accessor.WaitingForSupervisorAction();


            var items = waitingForSupervisorAction.Cast<SupervisorDashboardInterviewViewModel>().ToList();

            Assert.That(items, Has.Count.EqualTo(4));
            items.Should().ContainSingle(i => i.InterviewId == Id.g1, $"Should contain interview in {InterviewStatus.RejectedBySupervisor} status and responsible supervisor");
            items.Should().ContainSingle(i => i.InterviewId == Id.g2, $"Should contain interview in {InterviewStatus.Completed} status");
            items.Should().ContainSingle(i => i.InterviewId == Id.g3, $"Should contain interview in {InterviewStatus.InterviewerAssigned} status and responsible supervisor");
            items.Should().ContainSingle(i => i.InterviewId == Id.g4, $"Should contain interview in {InterviewStatus.RejectedByHeadquarters} status and responsible supervisor");
        }

        [Test]
        public void should_put_items_in_outbox()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();

            var currentSupervisorId = Id.gA;
            var interviewerId = Id.gB;
            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == currentSupervisorId));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: interviewerId, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.ApprovedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: interviewerId, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g3, responsibleId: interviewerId, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedByHeadquarters));
            
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g9, responsibleId: currentSupervisorId, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g10, responsibleId: currentSupervisorId, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString(), status: InterviewStatus.RejectedByHeadquarters));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            var outbox = accessor.Outbox().ToList();

            // Assert
            var itemsCount = outbox.Count;
            var repositoryCount = accessor.OutboxCount();
            Assert.AreEqual(itemsCount, repositoryCount);

            var items = outbox.Cast<SupervisorDashboardInterviewViewModel>().ToList();

            Assert.That(items, Has.Count.EqualTo(3));
            items.Should().ContainSingle(i => i.InterviewId == Id.g1, $"Should contain interview in {InterviewStatus.ApprovedBySupervisor} status ");
            items.Should().ContainSingle(i => i.InterviewId == Id.g2, $"Should contain interview in {InterviewStatus.RejectedBySupervisor} status");
            items.Should().ContainSingle(i => i.InterviewId == Id.g3, $"Should contain interview in {InterviewStatus.RejectedByHeadquarters} status ");
        }

        [Test]
        public void when_IsWaitingForSupervisorActionInterview_and_interview_is_waiting_for_supervisor_action_then_should_be_true()
        {
            //arrange
            var principal = Create.Service.Principal(Id.gA);

            var interviews = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gA, status: InterviewStatus.RejectedBySupervisor),
                Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: Id.gA, status: InterviewStatus.ApprovedBySupervisor));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            //act
            var isWaitingForSupervisorActionInterview = accessor.IsWaitingForSupervisorActionInterview(Id.g1);
            //assert
            Assert.That(isWaitingForSupervisorActionInterview, Is.True);
        }

        [Test]
        public void when_IsOutboxInterview_and_interview_is_in_outbox_then_should_be_true()
        {
            //arange
            var principal = Create.Service.Principal(Id.gA);

            var interviews = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gA, status: InterviewStatus.ApprovedBySupervisor),
                Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: Id.gA, status: InterviewStatus.ApprovedBySupervisor));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            //act
            var isOutboxInterview = accessor.IsOutboxInterview(Id.g1);
            //assert
            Assert.That(isOutboxInterview, Is.True);
        }

        private DashboardItemsAccessor CreateItemsAccessor(
            IPlainStorage<InterviewView> interviews = null,
            IAssignmentDocumentsStorage assignments = null,
            IPrincipal principal = null,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo = null
            )
        {
            var viewModelFactory = new Mock<IInterviewViewModelFactory>();
            viewModelFactory.Setup(x => x.GetNew<SupervisorDashboardInterviewViewModel>())
                .Returns(() => new SupervisorDashboardInterviewViewModel(Mock.Of<IServiceLocator>(),
                    Mock.Of<IAuditLogService>(),
                    Mock.Of<IViewModelNavigationService>()));

            return new DashboardItemsAccessor(

                interviews ?? new InMemoryPlainStorage<InterviewView>(),
                assignments ?? Mock.Of<IAssignmentDocumentsStorage>(x => x.LoadAll() == new List<AssignmentDocument>()),
                principal ?? Mock.Of<IPrincipal>(),
                identifyingQuestionsRepo ?? new InMemoryPlainStorage<PrefilledQuestionView>(),
                viewModelFactory.Object
            );
        }
    }
}
