using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Tests.Unit.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.VersionedReadSideRepositoryReaderExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        Establish context = () =>
        {
            readerMock = new Mock<IVersionedReadSideRepositoryReader<View>>();
        };

        Because of = () =>
            VersionedReadSideRepositoryReaderExtensions.GetById(readerMock.Object, Guid.Parse("11111111111111111111111111111111"), 1);

        It should_pass_string_11111111111111111111111111111111_to_reader_GetById_method = () =>
            readerMock.Verify(x => x.GetById("11111111111111111111111111111111", 1), Times.Once);

        private static Mock<IVersionedReadSideRepositoryReader<View>> readerMock;
    }
}
