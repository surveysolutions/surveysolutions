using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Cleaner;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.SyncCacher;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class PullDataProcessorTestContext
    {
        protected static PullDataProcessor CreatePullDataProcessor(ICommandService commandService=null, IJsonUtils jsonUtils = null)
        {
            return new PullDataProcessor(Mock.Of<IChangeLogManipulator>(), commandService??Mock.Of<ICommandService>(),
                Mock.Of<IViewFactory<LoginViewInput, LoginView>>(), Mock.Of<IPlainQuestionnaireRepository>(), Mock.Of<ICleanUpExecutor>(),
                Mock.Of<ILogger>(), Mock.Of<ISyncCacher>(), Mock.Of<IStringCompressor>(), jsonUtils ?? Mock.Of<IJsonUtils>());
        }
    }
}
