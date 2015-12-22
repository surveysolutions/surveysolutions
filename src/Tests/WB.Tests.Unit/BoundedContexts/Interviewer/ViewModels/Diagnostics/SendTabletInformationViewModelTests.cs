using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.Diagnostics
{
    [TestFixture]
    public class SendTabletInformationViewModelTests : MvxIoCSupportingTest
    {
        [SetUp]
        public void Init()
        {
            base.Setup();
        }

        [Test]
        public void CreateTabletInformation_when_everything_is_ok_the_backup_should_be_created()
        {
            var sendTabletInformationViewModel = this.CreateSendTabletInformationViewModel();

            sendTabletInformationViewModel.CreateTabletInformationCommand.Execute();

            Assert.That(sendTabletInformationViewModel.IsPackageBuild, Is.EqualTo(true));
        }

        [Test]
        public void SendTabletInformation_when_everything_is_ok_package_sending_attemps_should_be_marked_as_completed()
        {
            var sendTabletInformationViewModel = this.CreateSendTabletInformationViewModel();
            sendTabletInformationViewModel.InformationPackageContent = new byte[0];

            sendTabletInformationViewModel.SendTabletInformationCommand.Execute();

            Assert.That(sendTabletInformationViewModel.IsPackageBuild, Is.EqualTo(false));
            Assert.That(sendTabletInformationViewModel.IsPackageSendingAttemptCompleted, Is.EqualTo(true));
            Assert.That(sendTabletInformationViewModel.PackageSendingAttemptResponceText, Is.EqualTo(InterviewerUIResources.Troubleshooting_InformationPackageIsSuccessfullySent));
        }

        [Test]
        public void
            SendTabletInformation_when_package_failed_to_be_sent_should_responce_should_contain_exception_message()
        {
            var exceptionMessage = "message";
            var synchronizationServiceMock = new Mock<ISynchronizationService>();
            synchronizationServiceMock.Setup(
                x => x.SendTabletInformationAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(SynchronizationExceptionType.InternalServerError, exceptionMessage));

            var sendTabletInformationViewModel =
                this.CreateSendTabletInformationViewModel(synchronizationService: synchronizationServiceMock.Object);

            sendTabletInformationViewModel.InformationPackageContent = new byte[0];

            sendTabletInformationViewModel.SendTabletInformationCommand.Execute();

            Assert.That(sendTabletInformationViewModel.IsPackageBuild, Is.EqualTo(false));
            Assert.That(sendTabletInformationViewModel.IsPackageSendingAttemptCompleted, Is.EqualTo(true));
            Assert.That(sendTabletInformationViewModel.PackageSendingAttemptResponceText,
                Is.EqualTo(exceptionMessage));
        }

        [Test]
        public void DeleteTabletInformation_When_package_have_been_build_Then_package_should_be_hidden()
        {
            var sendTabletInformationViewModel = this.CreateSendTabletInformationViewModel();

            sendTabletInformationViewModel.IsPackageBuild = true;

            sendTabletInformationViewModel.DeleteTabletInformationCommand.Execute();

            Assert.That(sendTabletInformationViewModel.IsPackageBuild, Is.EqualTo(false));
        }

        private SendTabletInformationViewModel CreateSendTabletInformationViewModel(ITroubleshootingService troubleshootingService=null,
            ISynchronizationService synchronizationService = null)
        {
            return
                new SendTabletInformationViewModel(
                    troubleshootingService ?? Mock.Of<ITroubleshootingService>(_ => _.GetSystemBackup() == new byte[0]),
                    synchronizationService ?? Mock.Of<ISynchronizationService>(), Mock.Of<ILogger>());
        }
    }
}
