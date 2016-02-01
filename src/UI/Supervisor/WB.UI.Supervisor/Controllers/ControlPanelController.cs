using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Supervisor.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        public ControlPanelController(IServiceLocator serviceLocator, IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, ISettingsProvider settingsProvider,
             ITransactionManagerProvider transactionManagerProvider, IEventStoreApiService eventStoreApiService)
            : base(serviceLocator, brokenSyncPackagesStorage, commandService, globalInfo, logger, settingsProvider, transactionManagerProvider, eventStoreApiService) { }
    }
}