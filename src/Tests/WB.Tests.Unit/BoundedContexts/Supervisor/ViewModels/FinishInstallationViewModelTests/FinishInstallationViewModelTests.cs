using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
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
    }
}
