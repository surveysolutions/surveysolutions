using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_count_of_stored_view_called_and_one_view_is_actually_stored : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {
            ravenFilesStoreRepositoryAccessor = CreateRavenFilesStoreRepositoryAccessor();
            ravenFilesStoreRepositoryAccessor.Store(new TestableView(), "test");
        };

        Because of = () => { result = ravenFilesStoreRepositoryAccessor.Count(); };

        It should_Return_one = () =>
           result.ShouldEqual(1);


        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor(); 
        private static int result;
    }
}
