using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels.FinishInstallationViewModelTests
{
    [TestOf(typeof(FinishInstallationViewModel))]
    internal class FinishInstallationViewModelTests
    {
        [Test]
        public async Task when_scanning_barcode_and_returns_valid_url()
        {
            var endpoint = "https://example.com";

            var scanner = new Mock<IQRBarcodeScanService>();
            scanner.Setup(x => x.ScanAsync()).Returns(Task.FromResult<QRBarcodeScanResult>(new QRBarcodeScanResult()
            {
                Code = endpoint
            }));

            var viewModel = Create.ViewModel.FinishInstallationViewModel(qrBarcodeScanService: scanner.Object);
            await viewModel.ScanCommand.ExecuteAsync();


            Assert.That(viewModel.Endpoint, Is.EqualTo(endpoint));
        }

        [Test]
        public async Task when_scanning_barcode_that_returns_valid_long_url()
        {
            var endpoint = "https://example.com";

            var scanner = new Mock<IQRBarcodeScanService>();
            scanner.Setup(x => x.ScanAsync()).Returns(Task.FromResult<QRBarcodeScanResult>(new QRBarcodeScanResult()
            {
                Code = endpoint + "/blabla/1"
            }));

            var viewModel = Create.ViewModel.FinishInstallationViewModel(qrBarcodeScanService: scanner.Object);
            await viewModel.ScanCommand.ExecuteAsync();

            Assert.That(viewModel.Endpoint, Is.EqualTo(endpoint));
        }

        [Test]
        public async Task when_scanning_barcode_that_returns_invalid_url()
        {
            var endpoint = "invalid_url";

            var scanner = new Mock<IQRBarcodeScanService>();
            scanner.Setup(x => x.ScanAsync()).Returns(Task.FromResult<QRBarcodeScanResult>(new QRBarcodeScanResult()
            {
                Code = endpoint
            }));

            var viewModel = Create.ViewModel.FinishInstallationViewModel(qrBarcodeScanService: scanner.Object);
            await viewModel.ScanCommand.ExecuteAsync();
            
            Assert.That(viewModel.Endpoint, Is.Null);
            Assert.That(viewModel.ErrorMessage, Is.EqualTo("An error occurred while scanning"));
        }

        [Test]
        public async Task when_scanning_barcode_that_returns_object()
        {
            var endpoint = "https://example.com";
            var login = "username";

            var scanner = new Mock<IQRBarcodeScanService>();
            scanner.Setup(x => x.ScanAsync()).Returns(Task.FromResult<QRBarcodeScanResult>(new QRBarcodeScanResult()
            {
                Code = "dummy"
            }));

            var serializer = new Mock<ISerializer>();
            serializer.Setup(x => x.Deserialize<FinishInstallationInfo>(It.IsAny<string>()))
                .Returns(new FinishInstallationInfo()
                {
                    Url = endpoint,
                    Login = login
                });

            var viewModel = Create.ViewModel.FinishInstallationViewModel(qrBarcodeScanService: scanner.Object,
                serializer: serializer.Object);
            await viewModel.ScanCommand.ExecuteAsync();

            Assert.That(viewModel.Endpoint, Is.EqualTo(endpoint));
            Assert.That(viewModel.UserName, Is.EqualTo(login));
        }

        [Test]
        public async Task when_relink_is_triggered_should_navigate_to_relink_screen_with_supervisor_identity()
        {
            var supervisorId = Guid.NewGuid();
            var supervisorName = "supervisor1";
            var password = "pass1";
            var token = "tok1";
            var tenantId = "tenant1";
            var workspaceName = "primary";

            var supervisorApiView = new SupervisorApiView
            {
                Id = supervisorId,
                Email = "sv@example.com",
                Workspaces = new List<UserWorkspaceApiView>
                {
                    new UserWorkspaceApiView { Name = workspaceName }
                }
            };

            var syncService = new Mock<ISupervisorSynchronizationService>();
            syncService.Setup(x => x.LoginAsync(It.IsAny<LogonInfo>(), It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
            syncService.Setup(x => x.GetSupervisorAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(supervisorApiView);
            syncService.Setup(x => x.HasCurrentUserDeviceAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            syncService.Setup(x => x.CanSynchronizeAsync(It.IsAny<RestCredentials>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SynchronizationException(SynchronizationExceptionType.UserLinkedToAnotherDevice));
            syncService.Setup(x => x.GetTenantId(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenantId);

            RelinkDeviceViewModelArg capturedArg = null;
            var navigationService = new Mock<IViewModelNavigationService>();
            navigationService
                .Setup(x => x.NavigateToAsync<RelinkDeviceViewModel, RelinkDeviceViewModelArg>(It.IsAny<RelinkDeviceViewModelArg>(), It.IsAny<bool>()))
                .Callback<RelinkDeviceViewModelArg, bool>((arg, _) => capturedArg = arg)
                .Returns(Task.FromResult(true));

            var passwordHasher = new Mock<IPasswordHasher>();
            passwordHasher.Setup(x => x.Hash(password)).Returns("hashedPass");

            var principal = Mock.Of<IPrincipal>(x =>
                x.CurrentUserIdentity == Create.Other.SupervisorIdentity(null, supervisorName, null, supervisorId));

            var viewModel = Create.ViewModel.FinishInstallationViewModel(
                viewModelNavigationService: navigationService.Object,
                synchronizationService: syncService.Object,
                passwordHasher: passwordHasher.Object,
                principal: principal);

            viewModel.Endpoint = "https://example.com";
            viewModel.UserName = supervisorName;
            viewModel.Password = password;

            await viewModel.SignInCommand.ExecuteAsync();

            navigationService.Verify(
                x => x.NavigateToAsync<RelinkDeviceViewModel, RelinkDeviceViewModelArg>(It.IsAny<RelinkDeviceViewModelArg>(), It.IsAny<bool>()),
                Times.Once);
            Assert.That(capturedArg, Is.Not.Null);
            Assert.That(capturedArg.Identity, Is.Not.Null);
            Assert.That(capturedArg.Identity.Name, Is.EqualTo(supervisorName));
            Assert.That(capturedArg.Identity.Token, Is.EqualTo(token));
            Assert.That(capturedArg.Identity.TenantId, Is.EqualTo(tenantId));
            Assert.That(capturedArg.Identity.Workspace, Is.EqualTo(workspaceName));
        }
    }
}
