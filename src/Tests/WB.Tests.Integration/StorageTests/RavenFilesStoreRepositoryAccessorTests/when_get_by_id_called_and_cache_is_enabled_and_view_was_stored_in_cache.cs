using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_get_by_id_called_and_cache_is_enabled_and_view_was_stored_in_cache : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {

            ravenFilesStoreRepositoryAccessor.EnableCache();
            ravenFilesStoreRepositoryAccessor.Store(storedView, id);
        };

        Because of = () => { result = ravenFilesStoreRepositoryAccessor.GetById(id); };

        It should_return_not_null = () =>
           result.ShouldNotBeNull();

        It should_property_of_stored_view_be_equal_to_property_of_result_view = () =>
           result.RandomNumber.ShouldEqual(storedView.RandomNumber);

        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor();
        private static TestableView result;
        private static TestableView storedView = new TestableView() { RandomNumber = 2 };
        private static string id = "test";
    }
}
