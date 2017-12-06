using System.Collections.Generic;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    public class DataExportController: BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly InterviewDataExportSettings interviewDataExportSettings;

        public DataExportController(ICommandService commandService, ILogger logger,
            IAllUsersAndQuestionnairesFactory
                allUsersAndQuestionnairesFactory, InterviewDataExportSettings interviewDataExportSettings)
            : base(commandService, logger)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        [ObserverNotAllowed]
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.DataExport;
            this.ViewBag.EnableInterviewHistory = this.interviewDataExportSettings.EnableInterviewHistory;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load();

            ExportModel export = new ExportModel();
            export.Questionnaires = usersAndQuestionnaires.Questionnaires;
            export.ExportStatuses = new List<InterviewStatus>
            {
                InterviewStatus.InterviewerAssigned,
                InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.ApprovedByHeadquarters
            };

            return this.View(export);
        }
    }
}