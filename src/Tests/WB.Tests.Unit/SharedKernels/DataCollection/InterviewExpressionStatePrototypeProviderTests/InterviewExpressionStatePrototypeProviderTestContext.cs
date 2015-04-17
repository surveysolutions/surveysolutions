using System;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    [Subject(typeof(InterviewExpressionStatePrototypeProvider))]
    internal class InterviewExpressionStatePrototypeProviderTestContext
    {
        protected static InterviewExpressionStatePrototypeProvider CreateInterviewExpressionStatePrototype(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            return new InterviewExpressionStatePrototypeProvider(questionnareAssemblyFileAccessor, ServiceLocator.Current.GetInstance<IFileSystemAccessor>(), new InterviewExpressionStateFactory());
        }

        protected static Mock<IQuestionnaireAssemblyFileAccessor> CreateIQuestionnareAssemblyFileAccessorMock(string path)
        {
            var result = new Mock<IQuestionnaireAssemblyFileAccessor>();
            result.Setup(x => x.GetFullPathToAssembly(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(path);

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
