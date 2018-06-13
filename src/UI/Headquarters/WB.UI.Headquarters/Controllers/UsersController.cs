using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    [LimitsFilter]
    public class UsersController : BaseController
    {
        public UsersController(ICommandService commandService, ILogger logger) : base(commandService, logger)
        {
        }

        [ActivePage(MenuItem.UserBatchUpload)]
        [ObserverNotAllowed]
        public ActionResult Index() => View(new
        {
            Api = new
            {
                UploadUsersUrl = Url.Action("Index"),
                QuestionnairesUrl = Url.Action("Index", "SurveySetup"),
                ImportUsersTemplateUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "ImportUsersTemplate" }),
                ImportUsersUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "ImportUsers" }),
                ImportUsersStatusUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "ImportStatus" }),
                ImportUsersCompleteStatusUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "ImportCompleteStatus" }),
                ImportUsersCancelUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "UsersApi", action = "CancelToImportUsers" }),
                SupervisorCreateUrl = Url.Action("Create", "Supervisor"),
                InterviewerCreateUrl = Url.Action("Create", "Interviewer")
            },
            Config = new
            {
                AllowedUploadFileExtensions = new[] { TextExportFile.Extension, TabExportFile.Extention }
            }
        });
    }
}
