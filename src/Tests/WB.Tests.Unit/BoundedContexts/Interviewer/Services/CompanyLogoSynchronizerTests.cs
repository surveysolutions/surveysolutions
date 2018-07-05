using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services
{
    [TestFixture]
    [TestOf(typeof(CompanyLogoSynchronizer))]
    public class CompanyLogoSynchronizerTests
    {
        [Test]
        public async Task when_logo_is_synchronzed_it_should_be_stored_in_local_storage()
        {
            ISynchronizationService synchronizationService = Substitute.For<ISynchronizationService>();
            var logoContent = new byte[0];
            var etag = "etag";
            synchronizationService.GetCompanyLogo(null, CancellationToken.None)
                                  .ReturnsForAnyArgs(new CompanyLogoInfo
                                  {
                                      Etag = etag,
                                      HasCustomLogo = true,
                                      Logo = logoContent,
                                      LogoNeedsToBeUpdated = true
                                  });

            var logoStorage = new InMemoryPlainStorage<CompanyLogo>();
            var synchronizer = GetSynchronizer(synchronizationService, logoStorage);

            // act
            await synchronizer.DownloadCompanyLogo(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // assert
            var logo = logoStorage.FirstOrDefault();
            Assert.That(logo, Is.Not.Null);
            Assert.That(logo.ETag, Is.EqualTo(etag));
            Assert.That(logo.File, Is.SameAs(logoContent));
            Assert.That(logo.Id, Is.EqualTo(CompanyLogo.StorageKey));
        }

        [Test]
        public async Task when_local_logo_is_same_as_on_server_Should_not_override_it()
        {
            ISynchronizationService synchronizationService = Substitute.For<ISynchronizationService>();
            var etag = "etag";
            synchronizationService.GetCompanyLogo(null, CancellationToken.None)
                                  .ReturnsForAnyArgs(new CompanyLogoInfo
                                  {
                                      Etag = etag,
                                      HasCustomLogo = true,
                                      Logo = new byte[0],
                                      LogoNeedsToBeUpdated = false
                                  });

            var logoStorage = Substitute.For<IPlainStorage<CompanyLogo>>();
            logoStorage.GetById(CompanyLogo.StorageKey).Returns(new CompanyLogo
            {
                ETag = etag,
                File = new byte[] { 1, 2, 3, 5 },
                Id = CompanyLogo.StorageKey
            });
            var synchronizer = GetSynchronizer(synchronizationService, logoStorage);

            // act
            await synchronizer.DownloadCompanyLogo(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // assert
            logoStorage.DidNotReceiveWithAnyArgs().Store((CompanyLogo)null);
        }

        [Test]
        public async Task when_logo_is_removed_from_hq_Should_remove_logo_from_local_storage()
        {
            ISynchronizationService synchronizationService = Substitute.For<ISynchronizationService>();
            synchronizationService.GetCompanyLogo(null, CancellationToken.None)
                                  .ReturnsForAnyArgs(new CompanyLogoInfo
                                  {
                                      HasCustomLogo = false,
                                  });

            var logoStorage = new InMemoryPlainStorage<CompanyLogo>();
            logoStorage.Store(new CompanyLogo
            {
                ETag = "etag",
                File = new byte[] { 1, 2, 3, 5 },
                Id = CompanyLogo.StorageKey
            });
            var synchronizer = GetSynchronizer(synchronizationService, logoStorage);

            // act
            await synchronizer.DownloadCompanyLogo(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // assert
            Assert.That(logoStorage.GetById(CompanyLogo.StorageKey), Is.Null);
        }

        [Test]
        public async Task when_logo_updated_on_headquarters_Should_update_local_logo()
        {
            ISynchronizationService synchronizationService = Substitute.For<ISynchronizationService>();
            var hqLogoContent = new byte[] { 1, 3, 5 };
            var hqEtag = "updated";
            synchronizationService.GetCompanyLogo(null, CancellationToken.None)
                                  .ReturnsForAnyArgs(new CompanyLogoInfo
                                  {
                                      Etag = hqEtag,
                                      HasCustomLogo = true,
                                      Logo = hqLogoContent,
                                      LogoNeedsToBeUpdated = true
                                  });

            var logoStorage = new InMemoryPlainStorage<CompanyLogo>();
            logoStorage.Store(new CompanyLogo
            {
                ETag = "etag",
                File = new byte[] { 1, 2, 3, 5 },
                Id = CompanyLogo.StorageKey
            });
            var synchronizer = GetSynchronizer(synchronizationService, logoStorage);

            // act
            await synchronizer.DownloadCompanyLogo(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // assert
            var companyLogo = logoStorage.GetById(CompanyLogo.StorageKey);
            Assert.That(companyLogo.ETag, Is.EqualTo(hqEtag));
            Assert.That(companyLogo.File, Is.SameAs(hqLogoContent));
        }

        private CompanyLogoSynchronizer GetSynchronizer(ISynchronizationService synchronizationService, 
            IPlainStorage<CompanyLogo> logoStorage)
        {
            return new CompanyLogoSynchronizer(logoStorage, synchronizationService);
        }
    }
}
