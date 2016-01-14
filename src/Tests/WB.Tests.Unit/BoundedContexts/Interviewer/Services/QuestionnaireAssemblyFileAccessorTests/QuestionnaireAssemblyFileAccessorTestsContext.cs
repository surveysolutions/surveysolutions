using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.QuestionnaireAssemblyFileAccessorTests
{
    internal class QuestionnaireAssemblyFileAccessorTestsContext
    {
        public InterviewerQuestionnaireAssemblyFileAccessor QuestionnaireAssemblyFileAccessor(IFileSystemAccessor fileSystemAccessor = null,
            IAsynchronousFileSystemAccessor asyncFileSystemAccessor = null, ILogger logger = null, string pathToAssembliesDirectory = null)
        {
            return new InterviewerQuestionnaireAssemblyFileAccessor(
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                asyncFileSystemAccessor: asyncFileSystemAccessor ?? Mock.Of<IAsynchronousFileSystemAccessor>(),
                logger: logger ?? Mock.Of<ILogger>(),
                pathToAssembliesDirectory: pathToAssembliesDirectory ?? string.Empty);
        }
    }
}
