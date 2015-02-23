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
    internal class when_view_is_stored_in_old_format_without_version
    {
        Establish context = () =>
        {
            storedView = new EntityWithVersion(){Version = version};

            var internalStorage =
                Mock.Of<IReadSideStorage<EntityWithVersion>>(
                    _ => _.GetById(id) == storedView);
            readSideStorageVersionedWrapper =
                new ReadSideStorageVersionedWrapper<EntityWithVersion>(internalStorage);
        };

        Because of = () =>
            result = readSideStorageVersionedWrapper.Get(id, version);

        It should_returned_result_be_equal_stored_view_with_id_equal_to_passed_id_and_version_stored_inside_the_view = () =>
            result.ShouldEqual(storedView);

        private static EntityWithVersion result;
        private static EntityWithVersion storedView;
        private static ReadSideStorageVersionedWrapper<EntityWithVersion> readSideStorageVersionedWrapper;
        private static string id = "11111111111111111111111111111111";
        private static long version = 3;
    }

    public class EntityWithVersion : IReadSideRepositoryEntity
    {
        public long Version { get; set; }
    }
}
