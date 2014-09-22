using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.CapiDataSynchronizationServiceTests
{
    internal class CapiDataSynchronizationServiceTestContext
    {
        protected static CapiDataSynchronizationService CreateCapiDataSynchronizationService(IChangeLogManipulator changeLogManipulator,
            ICommandService commandService = null, IJsonUtils jsonUtils = null,
            IViewFactory<LoginViewInput, LoginView> loginViewFactory = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null, ICapiSynchronizationCacheService capiSynchronizationCacheService = null,
            ICapiCleanUpService capiCleanUpService = null, IQuestionnareAssemblyFileAccessor questionnareAssemblyFileAccessor = null)
        {
            var mockOfCompressor = new Mock<IStringCompressor>();
            mockOfCompressor.Setup(x => x.DecompressString(Moq.It.IsAny<string>())).Returns<string>(s => s);

            return new CapiDataSynchronizationService(changeLogManipulator, commandService ?? Mock.Of<ICommandService>(),
                loginViewFactory ?? Mock.Of<IViewFactory<LoginViewInput, LoginView>>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                capiCleanUpService ?? Mock.Of<ICapiCleanUpService>(),
                Mock.Of<ILogger>(), capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(), 
                mockOfCompressor.Object, jsonUtils ?? Mock.Of<IJsonUtils>(),
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnareAssemblyFileAccessor>());
        }
    }
}
