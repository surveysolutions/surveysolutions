using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireAssemblyFileAccessorTests
{
    [Subject(typeof(QuestionnaireAssemblyFileAccessor))]
    class QuestionnaireAssemblyFileAccessorTestsContext
    {
        protected static QuestionnaireAssemblyFileAccessor CreateQuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new QuestionnaireAssemblyFileAccessor(fileSystemAccessor ?? CreateIFileSystemAccessorMock().Object, "", "QuestionnaireAssembly");
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return result;
        }
    }
}
