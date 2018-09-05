using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Tests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.Diagnostics
{
    [TestFixture]
    internal class CheckNewVersionViewModelTests: MvxIoCSupportingTest
    {
        [SetUp]
        public void Init()
        {
            base.Setup();
        }

        [Test]
        public void UpdateApplication_When_update_is_successful_Then_check_new_version_should_be_finished_with_empty_result_message
            ()
        {
            var checkNewVersionViewModel = CreateCheckNewVersionViewModel();

            checkNewVersionViewModel.UpdateApplicationCommand.Execute();

            Assert.That(checkNewVersionViewModel.IsVersionCheckInProgress, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.CheckNewVersionResult, Is.Null);
        }

        [Test]
        public void UpdateApplication_When_update_throw_exception_Then_check_new_version_should_be_finished_with_exception_message_as_result_message
         ()
        {
            var exceptionMessage = "message";
            var tabletDiagnosticServiceMock= Substitute.For<ITabletDiagnosticService>();

            tabletDiagnosticServiceMock.UpdateTheApp(CancellationToken.None, true).ThrowsForAnyArgs(new Exception(exceptionMessage));

            var checkNewVersionViewModel = CreateCheckNewVersionViewModel(tabletDiagnosticService: tabletDiagnosticServiceMock);

            checkNewVersionViewModel.UpdateApplicationCommand.Execute();

            Assert.That(checkNewVersionViewModel.IsVersionCheckInProgress, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.CheckNewVersionResult, Is.EqualTo(exceptionMessage));
        }

        [Test]
        public void CheckVersion_When_new_version_is_absent_Then_check_new_version_should_be_finished_with_message_that_app_has_the_latest_Version()
        {
            var checkNewVersionViewModel = CreateCheckNewVersionViewModel();

            checkNewVersionViewModel.CheckVersionCommand.Execute();

            Assert.That(checkNewVersionViewModel.IsVersionCheckInProgress, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.IsNewVersionAvaliable, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.CheckNewVersionResult, Is.EqualTo(InterviewerUIResources.Diagnostics_YouHaveTheLatestVersionOfApplication));
        }

        [Test]
        public void CheckVersion_When_new_version_exists_Then_check_new_version_should_be_finished_with_message_that_app_could_be_updated()
        {
            var synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
            synchronizationServiceMock.Setup(x => x.GetLatestApplicationVersionAsync(Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<int?>(3));
            
            var checkNewVersionViewModel = CreateCheckNewVersionViewModel(
                synchronizationService: synchronizationServiceMock.Object);

            checkNewVersionViewModel.CheckVersionCommand.Execute();

            Assert.That(checkNewVersionViewModel.IsVersionCheckInProgress, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.IsNewVersionAvaliable, Is.EqualTo(true));
            Assert.That(checkNewVersionViewModel.CheckNewVersionResult, Is.Null);
        }

        [Test]
        public void CheckVersion_When_exception_have_throws_during_check_Then_check_new_version_should_be_finished_with_message_of_exception()
        {
            var exceptionMessage = "message";

            var synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
            synchronizationServiceMock.Setup(x => x.GetLatestApplicationVersionAsync(
                Moq.It.IsAny<CancellationToken>())).Throws(new Exception(exceptionMessage));

            var checkNewVersionViewModel = CreateCheckNewVersionViewModel(synchronizationService: synchronizationServiceMock.Object);

            checkNewVersionViewModel.CheckVersionCommand.Execute();

            Assert.That(checkNewVersionViewModel.IsVersionCheckInProgress, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.IsNewVersionAvaliable, Is.EqualTo(false));
            Assert.That(checkNewVersionViewModel.CheckNewVersionResult, Is.EqualTo(exceptionMessage));
        }

        [Test]
        public void RejectUpdateApplication_When_new_version_is_available_Then_new_version_should_be_hidded()
        {
            var checkNewVersionViewModel = CreateCheckNewVersionViewModel();

            checkNewVersionViewModel.IsNewVersionAvaliable = true;

            checkNewVersionViewModel.RejectUpdateApplicationCommand.Execute();
            
            Assert.That(checkNewVersionViewModel.IsNewVersionAvaliable, Is.EqualTo(false));
        }

        private CheckNewVersionViewModel CreateCheckNewVersionViewModel(
            IOnlineSynchronizationService synchronizationService = null,
            IInterviewerSettings interviewerSettings = null,
            ITabletDiagnosticService tabletDiagnosticService = null)
        {
            return new CheckNewVersionViewModel(
                synchronizationService ?? Mock.Of<IOnlineSynchronizationService>(),
                interviewerSettings ?? Mock.Of<IInterviewerSettings>(_ => _.Endpoint == "endpoint" && _.GetApplicationVersionCode()==1),
                tabletDiagnosticService ?? Mock.Of<ITabletDiagnosticService>(), Mock.Of<ILogger>());
        }
    }
}
