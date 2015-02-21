using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideStorageVersionedWrapperTests
{
    [Subject(typeof(ReadSideStorageVersionedWrapper<>))]
    internal class when_view_is_stored_in_old_format_without_version_and_version_property_is_missing_in_stored_view
    {
        Establish context = () =>
        {
            storedView = Mock.Of<IReadSideRepositoryEntity>();

            var internalStorage =
                Mock.Of<IReadSideStorage<IReadSideRepositoryEntity>>(
                    _ => _.GetById(id) == storedView);
            readSideStorageVersionedWrapper =
                new ReadSideStorageVersionedWrapper<IReadSideRepositoryEntity>(internalStorage);
        };

        Because of = () =>
            result = readSideStorageVersionedWrapper.Get(id, version);

        It should_returned_result_be_null = () =>
            result.ShouldBeNull();

        private static IReadSideRepositoryEntity result;
        private static IReadSideRepositoryEntity storedView;
        private static ReadSideStorageVersionedWrapper<IReadSideRepositoryEntity> readSideStorageVersionedWrapper;
        private static string id = "11111111111111111111111111111111";
        private static long version = 3;
    }
}
