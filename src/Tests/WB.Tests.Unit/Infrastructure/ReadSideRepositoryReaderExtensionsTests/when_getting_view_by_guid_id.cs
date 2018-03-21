using System;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.Infrastructure.ReadSideRepositoryReaderExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            readerMock = new Mock<IReadSideRepositoryReader<View>>();
            BecauseOf();
        }

        public void BecauseOf() =>
            ReadSideExtensions.GetById(readerMock.Object, Guid.Parse("11111111111111111111111111111111"));

        [NUnit.Framework.Test] public void should_pass_string_11111111111111111111111111111111_to_readers_GetById_method () =>
            readerMock.Verify(x => x.GetById("11111111111111111111111111111111"), Times.Once);

        private static Mock<IReadSideRepositoryReader<View>> readerMock;
    }
}
