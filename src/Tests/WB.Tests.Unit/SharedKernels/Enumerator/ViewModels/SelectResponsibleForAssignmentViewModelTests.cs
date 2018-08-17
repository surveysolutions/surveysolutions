using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(SelectResponsibleForAssignmentViewModel))]
    internal class SelectResponsibleForAssignmentViewModelTests
    {
        [Test]
        public void when_prepare_and_reassign_for_interview_should_list_of_interviewers_doesnot_exist_responsible_of_interview()
        {
            // arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var responsibleOfInterviewId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(Guid.NewGuid()),
                Create.Entity.InterviewerDocument(responsibleOfInterviewId));
            var statefullInterviewRepository = Setup.StatefulInterviewRepository(
                Create.AggregateRoot.StatefulInterview(interviewId, userId: responsibleOfInterviewId));

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(usersRepository: usersRepository,
                statefullInterviewRepository: statefullInterviewRepository);
            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(interviewId));
            // assert
            Assert.That(viewModel.UiItems.Select(x => x.Id), Does.Not.Contain(responsibleOfInterviewId));
        }

        [Test]
        public void when_prepare_and_reassign_for_assignement_should_list_of_interviewers_doesnot_exist_responsible_of_assignment()
        {
            // arrange
            var assignmentId = 22;
            var responsibleOfAssignmentId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(Guid.NewGuid()),
                Create.Entity.InterviewerDocument(responsibleOfAssignmentId));
            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                    Create.Entity.AssignmentDocument(assignmentId, responsibleId: responsibleOfAssignmentId).Build());

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(usersRepository: usersRepository, assignmentsStorage: assignmentsRepository);
            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(assignmentId));
            // assert
            Assert.That(viewModel.UiItems.Select(x => x.Id), Does.Not.Contain(responsibleOfAssignmentId));
        }

        [Test]
        public void when_prepare_and_interviewer_in_list_has_1_unlimited_assignment_then_InterviewsCount_should_be_1()
        {
            // arrange
            var assignmentResponsibleId = Guid.Parse("11111111111111111111111111111111");
            var responsibleInListId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(assignmentResponsibleId),
                Create.Entity.InterviewerDocument(responsibleInListId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(1, responsibleId: assignmentResponsibleId, quantity: 1).Build(),
                Create.Entity.AssignmentDocument(2, responsibleId: responsibleInListId, quantity: null).Build());

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository, 
                assignmentsStorage: assignmentsRepository);

            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(1));
            // assert
            Assert.That(viewModel.UiItems[0].InterviewsCount, Is.EqualTo(1));
        }

        [Test]
        public void when_prepare_and_interviewer_in_list_has_1_assignment_with_quantity_2_then_InterviewsCount_should_be_2()
        {
            // arrange
            var assignmentResponsibleId = Guid.Parse("11111111111111111111111111111111");
            var responsibleInList = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(assignmentResponsibleId),
                Create.Entity.InterviewerDocument(responsibleInList));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(1, responsibleId: assignmentResponsibleId, quantity: 1).Build(),
                Create.Entity.AssignmentDocument(2, responsibleId: responsibleInList, quantity: 2).Build());

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository);

            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(1));
            // assert
            Assert.That(viewModel.UiItems[0].InterviewsCount, Is.EqualTo(2));
        }

        [Test]
        public void when_prepare_and_interviewer_in_list_has_1_assignment_with_quantity_2_and_1_interview_by_that_assignment_then_InterviewsCount_should_be_1()
        {
            // arrange
            var assignmentResponsibleId = Guid.Parse("11111111111111111111111111111111");
            var responsibleInListId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(assignmentResponsibleId),
                Create.Entity.InterviewerDocument(responsibleInListId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(1, responsibleId: assignmentResponsibleId, quantity: 1).Build(),
                Create.Entity.AssignmentDocument(2, responsibleId: responsibleInListId, quantity: 2, interviewsCount: 1).Build());

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository);

            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(1));
            // assert
            Assert.That(viewModel.UiItems[0].InterviewsCount, Is.EqualTo(1));
        }

        [Test]
        public void when_prepare_and_interviewer_in_list_has_2_rejected_interviews_then_InterviewsCount_should_be_2()
        {
            // arrange
            var assignmentResponsibleId = Guid.Parse("11111111111111111111111111111111");
            var responsibleInListId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(assignmentResponsibleId),
                Create.Entity.InterviewerDocument(responsibleInListId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(1, responsibleId: assignmentResponsibleId, quantity: 1).Build());

            var interviewsRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewView(responsibleId: responsibleInListId),
                Create.Entity.InterviewView(responsibleId: responsibleInListId));

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository,
                interviewStorage: interviewsRepository);

            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(1));
            // assert
            Assert.That(viewModel.UiItems[0].InterviewsCount, Is.EqualTo(2));
        }

        [Test]
        public void when_prepare_and_interviewer_in_list_has_1_assignment_with_quantity_2_and_2_rejected_interviews_then_InterviewsCount_should_be_4()
        {
            // arrange
            var assignmentResponsibleId = Guid.Parse("11111111111111111111111111111111");
            var responsibleInListId = Guid.Parse("22222222222222222222222222222222");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(assignmentResponsibleId),
                Create.Entity.InterviewerDocument(responsibleInListId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(1, responsibleId: assignmentResponsibleId, quantity: 1).Build(),
                Create.Entity.AssignmentDocument(2, responsibleId: responsibleInListId, quantity: 2).Build());

            var interviewsRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewView(responsibleId: responsibleInListId),
                Create.Entity.InterviewView(responsibleId: responsibleInListId));

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository,
                interviewStorage: interviewsRepository);

            // act
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(1));
            // assert
            Assert.That(viewModel.UiItems[0].InterviewsCount, Is.EqualTo(4));
        }

        [Test]
        public async Task when_reassign_interview_then_responsible_of_interview_should_be_changed()
        {
            // arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var responsibleOfInterviewId = Guid.Parse("22222222222222222222222222222222");
            var newResponsibleId = Guid.Parse("33333333333333333333333333333333");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(newResponsibleId),
                Create.Entity.InterviewerDocument(responsibleOfInterviewId));
            var statefullInterviewRepository = Setup.StatefulInterviewRepository(
                Create.AggregateRoot.StatefulInterview(interviewId, userId: responsibleOfInterviewId));
            var mockOfCommandService = new Mock<ICommandService>();
            var mockOfNavigationViewModelService = new Mock<IViewModelNavigationService>();

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                statefullInterviewRepository: statefullInterviewRepository,
                commandService: mockOfCommandService.Object,
                navigationService: mockOfNavigationViewModelService.Object);
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(interviewId));
            viewModel.UiItems[0].IsSelected = true;
            viewModel.UiItems[0].SelectCommand.Execute();
            // act
            await viewModel.ReassignCommand.ExecuteAsync();
            // assert
            mockOfCommandService.Verify(x => x.Execute(
                    It.Is<AssignInterviewerCommand>(y => y.InterviewId == interviewId && y.InterviewerId == newResponsibleId), It.IsAny<string>()), Times.Once);

            mockOfNavigationViewModelService.Verify(x => x.NavigateToDashboardAsync(interviewId.FormatGuid()), Times.Once);
        }

        [Test]
        public async Task when_reassign_assignment_then_responsible_of_assignment_should_be_changed()
        {
            // arrange
            var assignmentId = 1;
            var responsibleOfAssignmentId = Guid.Parse("22222222222222222222222222222222");
            var newResponsibleId = Guid.Parse("33333333333333333333333333333333");
            var newResponsibleName = "responsible";

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(newResponsibleId, newResponsibleName),
                Create.Entity.InterviewerDocument(responsibleOfAssignmentId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(assignmentId, responsibleId: responsibleOfAssignmentId, quantity: 1).Build());
            var mockOfMvxMessenger = new Mock<IMvxMessenger>();

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository,
                mvxMessenger: mockOfMvxMessenger.Object);
            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(assignmentId));
            viewModel.UiItems[0].IsSelected = true;
            viewModel.UiItems[0].SelectCommand.Execute();
            // act
            await viewModel.ReassignCommand.ExecuteAsync();
            // assert
            var assignment = assignmentsRepository.GetById(assignmentId);

            Assert.That(assignment.ResponsibleId, Is.EqualTo(newResponsibleId));
            Assert.That(assignment.ResponsibleName, Is.EqualTo(newResponsibleName));
            Assert.That(assignment.ReceivedByInterviewerAt, Is.EqualTo(null));

            mockOfMvxMessenger.Verify(x => x.Publish(It.Is<DashboardChangedMsg>(y => y.Sender == viewModel)), Times.Once);
        }

        [Test]
        public void when_select_interviewer_should_CanReassign_should_be_true_and_prev_selected_interviewer_be_unckecked()
        {
            // arrange
            var assignmentId = 1;
            var responsibleOfAssignmentId = Guid.Parse("22222222222222222222222222222222");
            var newResponsibleId = Guid.Parse("33333333333333333333333333333333");

            var usersRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewerDocument(Guid.NewGuid()),
                Create.Entity.InterviewerDocument(newResponsibleId),
                Create.Entity.InterviewerDocument(responsibleOfAssignmentId));

            var assignmentsRepository = Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(
                Create.Entity.AssignmentDocument(assignmentId, responsibleId: responsibleOfAssignmentId, quantity: 1).Build());

            var viewModel = CreateSelectResponsibleForAssignmentViewModel(
                usersRepository: usersRepository,
                assignmentsStorage: assignmentsRepository);

            viewModel.Prepare(new SelectResponsibleForAssignmentArgs(assignmentId));
            viewModel.UiItems[0].IsSelected = true;
            viewModel.UiItems[0].SelectCommand.Execute();
            // act
            viewModel.UiItems[1].IsSelected = true;
            viewModel.UiItems[1].SelectCommand.Execute();
            // assert
            Assert.That(viewModel.UiItems[0].IsSelected, Is.False);
            Assert.That(viewModel.CanReassign, Is.True);
        }

        private static SelectResponsibleForAssignmentViewModel CreateSelectResponsibleForAssignmentViewModel(
            IMvxNavigationService mvxNavigationService = null,
            IPlainStorage<InterviewerDocument> usersRepository = null,
            IPrincipal principal = null,
            IAuditLogService auditLogService = null,
            ICommandService commandService = null,
            IViewModelNavigationService navigationService = null,
            IMvxMessenger mvxMessenger = null,
            IStatefulInterviewRepository statefullInterviewRepository = null,
            IPlainStorage<InterviewView> interviewStorage = null,
            IPlainStorage<AssignmentDocument, int> assignmentsStorage = null)
        {
            return new SelectResponsibleForAssignmentViewModel(
                mvxNavigationService: mvxNavigationService ?? Mock.Of<IMvxNavigationService>(),
                usersRepository: usersRepository ?? Mock.Of<IPlainStorage<InterviewerDocument>>(),
                principal: principal ?? Create.Service.Principal(Guid.NewGuid()),
                auditLogService: auditLogService ?? Mock.Of<IAuditLogService>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                navigationService: navigationService ?? Mock.Of<IViewModelNavigationService>(),
                mvxMessenger: mvxMessenger ?? Mock.Of<IMvxMessenger>(),
                statefullInterviewRepository: statefullInterviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                interviewStorage: interviewStorage ?? Create.Storage.SqliteInmemoryStorage<InterviewView>(),
                assignmentsStorage: assignmentsStorage ?? Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>());
        }
    }
}
