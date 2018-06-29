using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Dashboard
{
    [TestOf(typeof(DashboardItemsAccessor))]
    public class DashboardItemsAccessorTests
    {
        [Test]
        public void should_show_interviews_and_assignments_assigned_to_supervisor_in_waiting_for_decision_list()
        {
            var interviews = new InMemoryPlainStorage<InterviewView>();

            var principal = Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                                     x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Id.gA));

            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: Id.gA, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString()));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2, responsibleId: Id.gB, questionnaireId: Create.Entity.QuestionnaireIdentity().ToString()));

            var accessor = CreateItemsAccessor(interviews: interviews, principal: principal);

            // Act
            List<IDashboardItem> waitingForSupervisorAction = accessor.WaitingForSupervisorAction().ToList();

            Assert.That(waitingForSupervisorAction, Has.Count.EqualTo(1));

            SupervisorDashboardInterviewViewModel model = (SupervisorDashboardInterviewViewModel)waitingForSupervisorAction.First();
            Assert.That(model.InterviewId, Is.EqualTo(Id.g1));
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
                .Returns(new SupervisorDashboardInterviewViewModel(Mock.Of<IServiceLocator>(),
                    Mock.Of<IAuditLogService>(),
                    Mock.Of<IViewModelNavigationService>()));

            return new DashboardItemsAccessor(

                interviews ?? new InMemoryPlainStorage<InterviewView>(),
                assignments ?? Mock.Of<IAssignmentDocumentsStorage>(),
                principal ?? Mock.Of<IPrincipal>(),
                identifyingQuestionsRepo ?? new InMemoryPlainStorage<PrefilledQuestionView>(),
                viewModelFactory.Object
            );
        }
    }
}
