using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorBinaryHandler))]
    public class SupervisorBinaryHandlerTests
    {
        private Fixture fixture;

        [SetUp]
        public void Setup()
        {
            this.fixture = Create.Other.AutoFixture();
        }

        [Test]
        public async Task should_return_logo_updated_if_etag_differ()
        {
            fixture.Register<IPlainStorage<CompanyLogo>>(() =>
            {
                var storage = new InMemoryPlainStorage<CompanyLogo>();

                storage.inMemroyStorage[CompanyLogo.StorageKey] = new CompanyLogo
                {
                    ETag = "Sd",
                    File = new byte[200]
                };

                return storage;
            });

            var handler = fixture.Create<SupervisorBinaryHandler>();

            var response =await handler.GetCompanyLogo(new GetCompanyLogoRequest() {Etag = "11"});

            Assert.That(response.LogoInfo.Etag, Is.EqualTo("Sd"));
            Assert.That(response.LogoInfo.HasCustomLogo, Is.EqualTo(true));
            Assert.That(response.LogoInfo.LogoNeedsToBeUpdated, Is.EqualTo(true));
            Assert.That(response.LogoInfo.Logo, Is.Not.Empty);
        }

        [Test]
        public async Task should_not_return_logo_binary_if_etag_match()
        {
            fixture.Register<IPlainStorage<CompanyLogo>>(() =>
            {
                var storage = new InMemoryPlainStorage<CompanyLogo>();

                storage.inMemroyStorage[CompanyLogo.StorageKey] = new CompanyLogo
                {
                    ETag = "match",
                    File = new byte[200]
                };

                return storage;
            });

            var handler = fixture.Create<SupervisorBinaryHandler>();

            var response =await handler.GetCompanyLogo(new GetCompanyLogoRequest() {Etag = "match" });

            Assert.That(response.LogoInfo.Etag, Is.EqualTo("match"));
            Assert.That(response.LogoInfo.HasCustomLogo, Is.EqualTo(true));
            Assert.That(response.LogoInfo.LogoNeedsToBeUpdated, Is.EqualTo(false));
            Assert.That(response.LogoInfo.Logo, Is.Null);
        }

        [Test]
        public async Task should_return_logo_not_exists_if_no_logo()
        {
            fixture.Register<IPlainStorage<CompanyLogo>>(() => new InMemoryPlainStorage<CompanyLogo>());

            var handler = fixture.Create<SupervisorBinaryHandler>();

            var response =await handler.GetCompanyLogo(new GetCompanyLogoRequest() {Etag = "non_existing" });

            Assert.That(response.LogoInfo.Etag, Is.Null);
            Assert.That(response.LogoInfo.HasCustomLogo, Is.EqualTo(false));
            Assert.That(response.LogoInfo.LogoNeedsToBeUpdated, Is.EqualTo(true));
            Assert.That(response.LogoInfo.Logo, Is.Null);
        }
    }
}
