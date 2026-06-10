using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.AssignmentDashboardItemViewModelTests
{
    [TestOf(typeof(AssignmentDashboardItemViewModel))]
    public class AssignmentDashboardItemViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            base.Setup();
            Ioc.RegisterSingleton(Stub.MvxMainThreadAsyncDispatcher());
        }

        [Test]
        [SetUICulture("en-US")]
        public void when_initialized_should_build_titles()
        {
            var viewModel = CreateViewModel();

            // Act 
            viewModel.Init(Create.Entity.AssignmentDocument(12, quantity: 3, interviewsCount: 1,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity().ToString())
                .WithTitle("Questionnaire title")
                .Build());

            // Assrt
            Assert.That(viewModel, Has.Property(nameof(viewModel.Title)).EqualTo("Questionnaire title (v7)"));
            Assert.That(viewModel, Has.Property(nameof(viewModel.IdLabel)).EqualTo("#12"));
            Assert.That(viewModel, Has.Property(nameof(viewModel.SubTitle)).EqualTo("To collect: 2 interview(s)"));
        }

        [Test]
        [SetUICulture("en-US")]
        public void complete_assignment_dialog_message_should_match_hq_text()
        {
            Assert.That(EnumeratorUIResources.Dashboard_CompleteAssignment_Message,
                Is.EqualTo("Are you sure you have enumerated all eligible units?"));
        }

        [Test]
        [SetUICulture("en-US")]
        public void reopen_assignment_dialog_message_should_not_be_empty()
        {
            Assert.That(EnumeratorUIResources.Dashboard_ReopenAssignment_Message,
                Is.EqualTo("The assignment will be set back to Open status and interviewers will be able to create new interviews."));
        }

        AssignmentDashboardItemViewModel CreateViewModel()
        {
            var settings = Mock.Of<IEnumeratorSettings>(s =>
                s.AllowSupervisorChangeAssignmentStatus == true &&
                s.AllowInterviewerChangeAssignmentStatus == true);
            var serviceLocator = Mock.Of<IServiceLocator>(sl =>
                sl.GetInstance<IEnumeratorSettings>() == settings);

            return new SupervisorAssignmentDashboardItemViewModel(serviceLocator,
                Mock.Of<IMapInteractionService>(),
                Mock.Of<IViewModelNavigationService>(),
                Mock.Of<IUserInteractionService>());
        }
    }
}
