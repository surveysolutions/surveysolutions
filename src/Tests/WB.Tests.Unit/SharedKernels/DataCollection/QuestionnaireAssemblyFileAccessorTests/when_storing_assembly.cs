using System;
using Machine.Specifications;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    internal class when_storing_assembly : QuestionnaireAssemblyFileAccessorTestsContext
    {
        Establish context = () =>
        {
            questionnaireAssemblyFileAccessor = CreateQuestionnaireAssemblyFileAccessor(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireId, version, data1);

        It should_file_be_stored_on_file_system_once = () =>
            FileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name => name.Contains(questionnaireId.ToString())), expected), Times.Once);

        It should_file_be_marked_as_readonly_once = () =>
            FileSystemAccessorMock.Verify(x => x.MarkFileAsReadonly(Moq.It.Is<string>(name => name.Contains(questionnaireId.ToString()))), Times.Once);


        private static QuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static string data1 = "dGVzdA==";
        private static byte[] expected = Convert.FromBase64String(data1);

    }
}
