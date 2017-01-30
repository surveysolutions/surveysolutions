﻿using System;
using System.Reflection;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    [Subject(typeof(InterviewExpressionStatePrototypeProvider))]
    internal class InterviewExpressionStatePrototypeProviderTestContext
    {
        protected static InterviewExpressionStatePrototypeProvider CreateInterviewExpressionStatePrototype(IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            return new InterviewExpressionStatePrototypeProvider(questionnareAssemblyFileAccessor, ServiceLocator.Current.GetInstance<IFileSystemAccessor>(), new InterviewExpressionStateUpgrader());
        }

        protected static Mock<IQuestionnaireAssemblyFileAccessor> CreateIQuestionnareAssemblyFileAccessorMock(string path)
        {
            var result = new Mock<IQuestionnaireAssemblyFileAccessor>();
            
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
