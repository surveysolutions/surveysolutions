using System.IO;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireAssemblyFileAccessorTests
{
    [NUnit.Framework.TestOf(typeof(QuestionnaireAssemblyAccessor))]
    class QuestionnaireAssemblyFileAccessorTestsContext
    {
        protected static QuestionnaireAssemblyAccessor CreateQuestionnaireAssemblyFileAccessor(IAssemblyService assemblyService = null)
        {
            return new QuestionnaireAssemblyAccessor(assemblyService ?? CreateIAssemblyService().Object, Mock.Of<ILogger>());
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
