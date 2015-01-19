using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_remove_by_id_called_but_view_is_missing : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {
            ravenFilesStoreRepositoryAccessor = CreateRavenFilesStoreRepositoryAccessor();
        };

        Because of = () => { ravenFilesStoreRepositoryAccessor.Remove(id); };

        It should_return_null = () =>
           ravenFilesStoreRepositoryAccessor.GetById(id).ShouldBeNull();

        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor();

        private static TestableView storedView = new TestableView() { RandomNumber = 2 };
        private static string id = "test";
    }
}
