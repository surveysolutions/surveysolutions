using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Tests.Unit.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.VersionedReadSideRepositoryWriterExtensionsTests
{
    internal class when_storing_view_by_guid_id
    {
        Establish context = () =>
        {
            writerMock = new Mock<IVersionedReadSideRepositoryWriter<View>>();
            view = Mock.Of<View>();
        };

        Because of = () =>
            VersionedReadSideRepositoryWriterExtensions.Store(writerMock.Object, view, Guid.Parse("11111111111111111111111111111111"));

        It should_pass_string_11111111111111111111111111111111_to_writers_GetById_method = () =>
            writerMock.Verify(x => x.Store(view, "11111111111111111111111111111111"), Times.Once);

        private static Mock<IVersionedReadSideRepositoryWriter<View>> writerMock;
        private static View view;
    }
}
