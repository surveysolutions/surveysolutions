using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewExpressionStatePrototypeProviderTests
{
    [Subject(typeof(InterviewExpressionStatePrototypeProvider))]
    internal class InterviewExpressionStatePrototypeProviderTestContext
    {
        protected static InterviewExpressionStatePrototypeProvider CreateInterviewExpressionStatePrototype(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor, 
            IFileSystemAccessor fileSystemAccessor)
        {
            return new InterviewExpressionStatePrototypeProvider(questionnareAssemblyFileAccessor, fileSystemAccessor);
        }


        protected static Mock<IQuestionnaireAssemblyFileAccessor> CreateIQuestionnareAssemblyFileAccessorMock(string path)
        {
            var result = new Mock<IQuestionnaireAssemblyFileAccessor>();
            result.Setup(x => x.GetFullPathToAssembly(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(path);

            return result;
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessor()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.IsFileExists(Moq.It.IsAny<string>()))
                .Returns(true);

            return result;
        }


        protected static void RunInAnotherAppDomain(CrossAppDomainDelegate actionToRun)
        {
            var dom = AppDomain.CreateDomain("test", AppDomain.CurrentDomain.Evidence,
                           AppDomain.CurrentDomain.BaseDirectory, string.Empty, false);

            dom.DoCallBack(actionToRun);
            AppDomain.Unload(dom);
        }
    }
}
