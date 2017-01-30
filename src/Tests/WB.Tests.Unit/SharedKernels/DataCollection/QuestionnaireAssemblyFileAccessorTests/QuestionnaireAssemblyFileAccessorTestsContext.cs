using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    [Subject(typeof(QuestionnaireAssemblyFileAccessor))]
    class QuestionnaireAssemblyFileAccessorTestsContext
    {
        protected static QuestionnaireAssemblyFileAccessor CreateQuestionnaireAssemblyFileAccessor(IAssemblyService assemblyService = null)
        {
            return new QuestionnaireAssemblyFileAccessor(assemblyService ?? CreateIAssemblyService().Object);
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return result;
        }

        protected static Mock<IAssemblyService> CreateIAssemblyService()
        {
            var result = new Mock<IAssemblyService>();

            return result;
        }
    }
}
