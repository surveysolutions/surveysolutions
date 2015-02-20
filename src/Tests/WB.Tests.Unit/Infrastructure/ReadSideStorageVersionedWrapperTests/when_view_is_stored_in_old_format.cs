using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using Moq;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideStorageVersionedWrapperTests
{
    [Subject(typeof(ReadSideStorageVersionedWrapper<>))]
    internal class when_view_is_stored_in_old_format
    {
        Establish context = () =>
        {
            storedView = Mock.Of<IReadSideRepositoryEntity>();

            var internalStorage =
                Mock.Of<IReadSideStorage<IReadSideRepositoryEntity>>(
                    _ => _.GetById(string.Format("{0}-{1}", id, version)) == storedView);
            readSideStorageVersionedWrapper =
                new ReadSideStorageVersionedWrapper<IReadSideRepositoryEntity>(internalStorage);
        };
        
        Because of = () =>
            result = readSideStorageVersionedWrapper.Get(id,version);

        It should_returned_result_be_equal_stored_result_with_id_in_format_id_dash_version = () =>
            result.ShouldEqual(storedView);

        private static IReadSideRepositoryEntity result;
        private static IReadSideRepositoryEntity storedView;
        private static ReadSideStorageVersionedWrapper<IReadSideRepositoryEntity> readSideStorageVersionedWrapper;
        private static string id = "11111111111111111111111111111111";
        private static long version = 3;
    }
}
