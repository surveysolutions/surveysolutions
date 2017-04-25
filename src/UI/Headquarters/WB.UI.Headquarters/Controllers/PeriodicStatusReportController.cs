using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class PeriodicStatusReportController : BaseController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;

        public PeriodicStatusReportController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IUserViewFactory userViewFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
           
        }

    }
}
