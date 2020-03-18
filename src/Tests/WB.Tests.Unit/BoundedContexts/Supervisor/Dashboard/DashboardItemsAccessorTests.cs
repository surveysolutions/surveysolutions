﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
    public class DashboardItemsAccessorTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            base.Setup();
            Ioc.RegisterSingleton(Stub.MvxMainThreadAsyncDispatcher());
        }

        [Test]
        public void should_show_items_with_filled_received_at_date_in_the_sent_box()
        {
             var interviews = new SqliteInmemoryStorage<InterviewView>();

            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Id.gA));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, receivedByInterviewerAt: DateTime.UtcNow));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            var waitingForSupervisorAction = accessor.GetSentToInterviewerItems();

            var items = waitingForSupervisorAction.Cast<SupervisorDashboardInterviewViewModel>().ToList();

            Assert.That(items, Has.Count.EqualTo(1));
            items.Should().ContainSingle(i => i.InterviewId == Id.g1, $"Should contain interview in {InterviewStatus.RejectedBySupervisor} status and responsible supervisor");
        }

        [Test]
        public void should_show_interviews_and_assignments_assigned_to_supervisor_in_waiting_for_decision_list()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();

            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Id.gA));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gA, status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: Id.gB, status: InterviewStatus.Completed));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g3, responsibleId: Id.gA, status: InterviewStatus.InterviewerAssigned));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g4, responsibleId: Id.gA, status: InterviewStatus.RejectedByHeadquarters));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g5, responsibleId: Id.gB, status: InterviewStatus.RejectedByHeadquarters));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g9, responsibleId: Id.gA, status: InterviewStatus.ApprovedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g10, responsibleId: Id.gB, status: InterviewStatus.RejectedBySupervisor));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            IEnumerable<IDashboardItem> waitingForSupervisorAction = accessor.WaitingForSupervisorAction();


            var items = waitingForSupervisorAction.Cast<SupervisorDashboardInterviewViewModel>().ToList();

            Assert.That(items, Has.Count.EqualTo(5));
            items.Should().ContainSingle(i => i.InterviewId == Id.g1, $"Should contain interview in {InterviewStatus.RejectedBySupervisor} status and responsible supervisor");
            items.Should().ContainSingle(i => i.InterviewId == Id.g2, $"Should contain interview in {InterviewStatus.Completed} status");
            items.Should().ContainSingle(i => i.InterviewId == Id.g3, $"Should contain interview in {InterviewStatus.InterviewerAssigned} status and responsible supervisor");
            items.Should().ContainSingle(i => i.InterviewId == Id.g4, $"Should contain interview in {InterviewStatus.RejectedByHeadquarters} status and responsible supervisor");
            items.Should().ContainSingle(i => i.InterviewId == Id.g5, $"Should contain interview in {InterviewStatus.RejectedByHeadquarters} status and responsible interviewer");
        }

        [Test]
        public void should_put_items_in_outbox()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();

            var currentSupervisorId = Id.gA;
            var interviewerId = Id.gB;
            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == currentSupervisorId));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: interviewerId, status: InterviewStatus.ApprovedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: interviewerId, status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g4, responsibleId: interviewerId, status: InterviewStatus.InterviewerAssigned));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g5, responsibleId: interviewerId, status: InterviewStatus.Restarted));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g7, responsibleId: interviewerId, status: InterviewStatus.RejectedBySupervisor, receivedByInterviewerAt: DateTime.UtcNow));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g8, responsibleId: interviewerId, status: InterviewStatus.RejectedByHeadquarters));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g9, responsibleId: currentSupervisorId, status: InterviewStatus.RejectedBySupervisor));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g10, responsibleId: currentSupervisorId, status: InterviewStatus.RejectedByHeadquarters));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            var outbox = accessor.Outbox().ToList();

            // Assert
            var itemsCount = outbox.Count;
            var repositoryCount = accessor.OutboxCount();
            Assert.AreEqual(itemsCount, repositoryCount);

            var items = outbox.Cast<SupervisorDashboardInterviewViewModel>().ToList();

            Assert.That(items, Has.Count.EqualTo(4));
            items.Should().ContainSingle(i => i.InterviewId == Id.g1, $"Should contain interview in {InterviewStatus.ApprovedBySupervisor} status ");
            items.Should().ContainSingle(i => i.InterviewId == Id.g2, $"Should contain interview in {InterviewStatus.RejectedBySupervisor} status");
            items.Should().ContainSingle(i => i.InterviewId == Id.g4, $"Should contain interview in {InterviewStatus.InterviewerAssigned} status ");
            items.Should().ContainSingle(i => i.InterviewId == Id.g5, $"Should contain interview in {InterviewStatus.Restarted} status ");
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

        [Test]
        public void IsSentToInterviewer_when_interview_is_sent_should_return_true()
        {
            var interview = Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gB,
                status: InterviewStatus.InterviewerAssigned, receivedByInterviewerAt: DateTime.UtcNow);

            var interviews = Create.Storage.SqliteInmemoryStorage(interview);

            var accessor = CreateItemsAccessor(interviews: interviews, principal: Create.Service.Principal(Id.gA));

            //act
            var isSentToInterviewer = accessor.IsSentToInterviewer(Id.g1);

            //assert
            Assert.That(isSentToInterviewer, Is.True);
        }

        private DashboardItemsAccessor CreateItemsAccessor(
            IPlainStorage<InterviewView> interviews = null,
            IAssignmentDocumentsStorage assignments = null,
            IPrincipal principal = null,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo = null,
            IPlainStorage<InterviewerDocument> interviewers = null
            )
        {
            var viewModelFactory = new Mock<IInterviewViewModelFactory>();
            viewModelFactory.Setup(x => x.GetNew<SupervisorDashboardInterviewViewModel>())
                .Returns(() => new SupervisorDashboardInterviewViewModel(Mock.Of<IServiceLocator>(),
                    Mock.Of<IAuditLogService>(),
                    Mock.Of<IViewModelNavigationService>(),
                    Create.Other.SupervisorPrincipal(),
                    interviewers ?? Mock.Of<IPlainStorage<InterviewerDocument>>(x => x.GetById(It.IsAny<string>()) == new InterviewerDocument())));

            return new DashboardItemsAccessor(

                interviews ?? Create.Storage.InMemorySqlitePlainStorage<InterviewView>(),
                assignments ?? Mock.Of<IAssignmentDocumentsStorage>(x => x.LoadAll() == new List<AssignmentDocument>()),
                principal ?? Mock.Of<IPrincipal>(),
                identifyingQuestionsRepo ?? Create.Storage.InMemorySqlitePlainStorage<PrefilledQuestionView>(),
                viewModelFactory.Object
            );
        }
    }
}
