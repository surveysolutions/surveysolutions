using System;
using System.Linq;
using System.ServiceModel;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Supervisor.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : WB.Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(serviceLocator, incomePackagesRepository, commandService, globalInfo, logger) {}
    }
}