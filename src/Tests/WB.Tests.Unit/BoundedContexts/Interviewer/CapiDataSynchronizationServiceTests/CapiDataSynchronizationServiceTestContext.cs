using Moq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.CapiDataSynchronizationServiceTests
{
    internal class CapiDataSynchronizationServiceTestContext
    {
        protected static CapiDataSynchronizationService CreateCapiDataSynchronizationService(IChangeLogManipulator changeLogManipulator = null,
            ICommandService commandService = null, 
            IJsonUtils jsonUtils = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null, 
            ICapiSynchronizationCacheService capiSynchronizationCacheService = null,
            ICapiCleanUpService capiCleanUpService = null, 
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IStringCompressor stringCompressor = null,
            IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> interviewMetaInfoFactory = null,
            IPrincipal principal = null)
        {
            var mockOfCompressor = new Mock<IStringCompressor>();
            mockOfCompressor.Setup(x => x.DecompressString(Moq.It.IsAny<string>())).Returns<string>(s => s);

            return new CapiDataSynchronizationService(changeLogManipulator ?? Mock.Of<IChangeLogManipulator>(), 
                commandService ?? Mock.Of<ICommandService>(),
                capiCleanUpService ?? Mock.Of<ICapiCleanUpService>(),
                Mock.Of<ILogger>(), 
                capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                interviewMetaInfoFactory ?? Mock.Of<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>(),
                principal ?? Mock.Of<IPrincipal>());
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
