using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Tests.Unit.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.VersionedReadSideRepositoryWriterExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        Establish context = () =>
        {
            writerMock = new Mock<IVersionedReadSideRepositoryWriter<View>>();
        };

        Because of = () =>
            VersionedReadSideRepositoryWriterExtensions.GetById(writerMock.Object, Guid.Parse("11111111111111111111111111111111"), 1);

        It should_pass_string_11111111111111111111111111111111_to_writers_GetById_method = () =>
            writerMock.Verify(x => x.GetById("11111111111111111111111111111111", 1), Times.Once);

        private static Mock<IVersionedReadSideRepositoryWriter<View>> writerMock;
    }
}
