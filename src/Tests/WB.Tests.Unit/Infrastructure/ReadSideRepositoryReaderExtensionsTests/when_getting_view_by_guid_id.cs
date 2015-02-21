using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideRepositoryReaderExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        Establish context = () =>
        {
            readerMock = new Mock<IReadSideRepositoryReader<View>>();
        };

        Because of = () =>
            ReadSideExtensions.GetById(readerMock.Object, Guid.Parse("11111111111111111111111111111111"));

        It should_pass_string_11111111111111111111111111111111_to_readers_GetById_method = () =>
            readerMock.Verify(x => x.GetById("11111111111111111111111111111111"), Times.Once);

        private static Mock<IReadSideRepositoryReader<View>> readerMock;
    }
}
