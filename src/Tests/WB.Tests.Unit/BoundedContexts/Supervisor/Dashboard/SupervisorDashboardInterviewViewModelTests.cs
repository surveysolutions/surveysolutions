using System.Collections.Generic;
using Moq;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Dashboard
{
    [TestOf(typeof(SupervisorDashboardInterviewViewModel))]
    public class SupervisorDashboardInterviewViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            base.Setup();
            Ioc.RegisterSingleton(Stub.MvxMainThreadAsyncDispatcher());
        }

        [Test]
        public void BindTitles_Should_put_current_supervisor_as_responsible_name_if_interview_is_assigned_to_him()
        {
            var interviewId = Id.g1;
            var supervisorId = Id.gA;
            var supervisorName = "supervisor";

            var principal = Mock.Of<IPrincipal>(x =>
                x.CurrentUserIdentity == Create.Other.SupervisorIdentity(supervisorId.FormatGuid(), supervisorName, null));

            var interview = Create.Entity.InterviewView(interviewId: interviewId);

            var viewModel = Create.ViewModel.SupervisorDashboardInterviewViewModel(principal: principal);

            // Act
            viewModel.Init(interview, new List<PrefilledQuestion>());

            // Assert
            Assert.That(viewModel, Has.Property(nameof(viewModel.Responsible)).EqualTo(supervisorName));
        }
    }
}
