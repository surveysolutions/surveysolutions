using System;
using System.Linq;
using System.ServiceModel;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Supervisor.Controllers
{
    [NoTransaction]
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : WB.Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        public ControlPanelController(IServiceLocator serviceLocator, IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, ISettingsProvider settingsProvider)
            : base(serviceLocator, brokenSyncPackagesStorage, commandService, globalInfo, logger, settingsProvider) { }
    }
}