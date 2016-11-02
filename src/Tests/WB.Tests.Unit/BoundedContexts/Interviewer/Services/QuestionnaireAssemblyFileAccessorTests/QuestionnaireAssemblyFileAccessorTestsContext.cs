using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.QuestionnaireAssemblyFileAccessorTests
{
    internal class QuestionnaireAssemblyFileAccessorTestsContext
    {
        public static InterviewerQuestionnaireAssemblyFileAccessor CreateQuestionnaireAssemblyFileAccessor(
            IFileSystemAccessor fileSystemAccessor = null, ILogger logger = null, string pathToAssembliesDirectory = null)
        {
            return new InterviewerQuestionnaireAssemblyFileAccessor(
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                logger: logger ?? Mock.Of<ILogger>(),
                pathToAssembliesDirectory: pathToAssembliesDirectory ?? string.Empty);
        }
    }
}
