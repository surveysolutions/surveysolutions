using System;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.Infrastructure.ReadSideStorageExtensionsTests
{
    internal class when_getting_view_by_guid_id
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            writerMock = new Mock<IReadSideRepositoryWriter<View>>();
            BecauseOf();
        }

        public void BecauseOf() => 
            ReadSideExtensions.GetById(writerMock.Object, Guid.Parse("11111111111111111111111111111111"));

        [NUnit.Framework.Test] public void should_pass_string_11111111111111111111111111111111_to_writers_GetById_method () =>
            writerMock.Verify(x => x.GetById("11111111111111111111111111111111"), Times.Once);
        
        private static Mock<IReadSideRepositoryWriter<View>> writerMock;
    }
}
