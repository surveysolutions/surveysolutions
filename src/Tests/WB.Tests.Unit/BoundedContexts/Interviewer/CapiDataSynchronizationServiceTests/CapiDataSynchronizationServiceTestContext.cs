﻿using Moq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.BoundedContexts.Interviewer.Views.Login;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.CapiDataSynchronizationServiceTests
{
    internal class CapiDataSynchronizationServiceTestContext
    {
        protected static CapiDataSynchronizationService CreateCapiDataSynchronizationService(IChangeLogManipulator changeLogManipulator = null,
            ICommandService commandService = null, 
            IJsonUtils jsonUtils = null,
            IViewFactory<LoginViewInput, LoginView> loginViewFactory = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null, 
            ICapiSynchronizationCacheService capiSynchronizationCacheService = null,
            ICapiCleanUpService capiCleanUpService = null, 
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IStringCompressor stringCompressor = null,
            IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> interviewMetaInfoFactory = null)
        {
            var mockOfCompressor = new Mock<IStringCompressor>();
            mockOfCompressor.Setup(x => x.DecompressString(Moq.It.IsAny<string>())).Returns<string>(s => s);

            return new CapiDataSynchronizationService(changeLogManipulator ?? Mock.Of<IChangeLogManipulator>(), 
                commandService ?? Mock.Of<ICommandService>(),
                loginViewFactory ?? Mock.Of<IViewFactory<LoginViewInput, LoginView>>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                capiCleanUpService ?? Mock.Of<ICapiCleanUpService>(),
                Mock.Of<ILogger>(), 
                capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                interviewMetaInfoFactory ?? Mock.Of<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>(),
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                Mock.Of<IQuestionnaireModelBuilder>());
        }

        protected static string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.None, settings);
        }

    }
}
