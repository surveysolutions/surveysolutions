using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.ReadSideStorageStrategyExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        Establish context = () =>
        {
            writerMock = new Mock<IStorageStrategy<View>>();
        };

        Because of = () =>
            ReadSideStorageStrategyExtensions.Select(writerMock.Object, Guid.Parse("11111111111111111111111111111111"));

        It should_pass_string_11111111111111111111111111111111_to_writers_GetById_method = () =>
            writerMock.Verify(x => x.Select("11111111111111111111111111111111"), Times.Once);

        private static Mock<IStorageStrategy<View>> writerMock;
    }
}
