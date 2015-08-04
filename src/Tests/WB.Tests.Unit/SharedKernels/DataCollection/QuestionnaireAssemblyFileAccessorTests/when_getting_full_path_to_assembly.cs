using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_getting_full_path_to_assembly : QuestionnaireAssemblyFileAccessorTestsContext
    {
        Establish context = () =>
        {
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => questionnaireAssemblyFileAccessor.GetFullPathToAssembly(questionnaireId, version);

        It should_combine_path_called = () =>
            FileSystemAccessorMock.Verify(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.Is<string>(name => name.Contains(questionnaireId.ToString()))), Times.Once);

        private static QuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

    }
}
