using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.ReadSideStorageStrategyExtensionsTests
{
    internal class when_adding_or_updating_view_by_guid_id
    {
        Establish context = () =>
        {
            writerMock = new Mock<IStorageStrategy<View>>();
            view = Mock.Of<View>();
        };

        Because of = () =>
            ReadSideStorageStrategyExtensions.AddOrUpdate(writerMock.Object, view, Guid.Parse("11111111111111111111111111111111"));

        It should_pass_string_11111111111111111111111111111111_to_writers_AddOrUpdate_method = () =>
            writerMock.Verify(x => x.AddOrUpdate(view, "11111111111111111111111111111111"), Times.Once);

        private static Mock<IStorageStrategy<View>> writerMock;
        private static View view;
    }
}