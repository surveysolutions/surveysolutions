using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_store_view_cache_is_enabled_and_limit_of_cached_size_is_reached : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {
            ravenFilesStoreRepositoryAccessor = CreateRavenFilesStoreRepositoryAccessor();
            ravenFilesStoreRepositoryAccessor.EnableCache();
            for (int i = 0; i < cacheLimit; i++)
            {
                ravenFilesStoreRepositoryAccessor.Store(new TestableView() { RandomNumber = i }, i.ToString());
            }
        };

        Because of = () => { ravenFilesStoreRepositoryAccessor.Store(new TestableView() { RandomNumber = 1804 }, id); };

        It should_return_view_count_equlas_to_count_of_stored_views = () =>
           ravenFilesStoreRepositoryAccessor.Count().ShouldEqual(30);

        It should_return_readable_status_with_disabled_cache_and_zero_cached_items = () =>
         ravenFilesStoreRepositoryAccessor.GetReadableStatus().ShouldEqual("cache enabled;    cached raven file storage items: 227;");

        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor();

        private static int cacheLimit = 256;
        private static string id = "nastya_id";
    }
}
