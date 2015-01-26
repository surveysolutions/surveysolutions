using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideStorageExtensionsTests
{
    internal class when_storing_view_by_guid_id
    {
        Establish context = () =>
        {
            writerMock = new Mock<IReadSideRepositoryWriter<View>>();
            view = Mock.Of<View>();
        };

        Because of = () =>
            ReadSideExtensions.Store(writerMock.Object, view, Guid.Parse("11111111111111111111111111111111"));

        It should_pass_string_11111111111111111111111111111111_to_writers_GetById_method = () =>
            writerMock.Verify(x => x.Store(view, "11111111111111111111111111111111"), Times.Once);

        private static Mock<IReadSideRepositoryWriter<View>> writerMock;
        private static View view;
    }
}