using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_disable_cache_method_called : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {
            ravenFilesStoreRepositoryAccessor = CreateRavenFilesStoreRepositoryAccessor();
            ravenFilesStoreRepositoryAccessor.EnableCache();
            for (int i = 0; i < viewCount; i++)
            {
                ravenFilesStoreRepositoryAccessor.Store(new TestableView() {RandomNumber = i}, i.ToString());
            }
        };

        Because of = () => { ravenFilesStoreRepositoryAccessor.DisableCache(); };

        It should_return_view_count_equlas_to_count_of_stored_views = () =>
           ravenFilesStoreRepositoryAccessor.Count().ShouldEqual(viewCount);

        It should_return_readable_status_with_disabled_cache_and_zero_cached_items = () =>
         ravenFilesStoreRepositoryAccessor.GetReadableStatus().ShouldEqual("cache disabled;    cached raven file storage items: 0;");

        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor();

        private static int viewCount = 40;
    }
}
