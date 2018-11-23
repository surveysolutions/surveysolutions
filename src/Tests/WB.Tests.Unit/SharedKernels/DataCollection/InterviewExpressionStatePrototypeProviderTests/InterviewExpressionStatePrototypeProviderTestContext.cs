using System;
using System.Reflection;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    [NUnit.Framework.TestOf(typeof(InterviewExpressionStatePrototypeProvider))]
    internal class InterviewExpressionStatePrototypeProviderTestContext
    {
        protected static InterviewExpressionStatePrototypeProvider CreateInterviewExpressionStatePrototype(IQuestionnaireAssemblyAccessor questionnareAssemblyFileAccessor)
        {
            return new InterviewExpressionStatePrototypeProvider(questionnareAssemblyFileAccessor, 
                new InterviewExpressionStateUpgrader(),
                Mock.Of<ILoggerProvider>());
        }

        protected static Mock<IQuestionnaireAssemblyAccessor> CreateIQuestionnareAssemblyFileAccessorMock(string path)
        {
            var result = new Mock<IQuestionnaireAssemblyAccessor>();
            
            result.Setup(x => x.IsQuestionnaireAssemblyExists(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>())).Returns(true);
            result.Setup(x => x.LoadAssembly(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>())).Returns(Assembly.LoadFrom(path));

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
