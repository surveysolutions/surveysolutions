using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Models.DataExport;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    //[LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class DataExportController: Controller
    {
        private readonly ExternalStoragesSettings externalStoragesSettings;
        private readonly IVirtualPathService pathService;

        public DataExportController(ExternalStoragesSettings externalStoragesSettings, 
            IVirtualPathService pathService)
        {
            this.externalStoragesSettings = externalStoragesSettings;
            this.pathService = pathService;
        }

        [ObservingNotAllowed]
        public ActionResult New()
        {
            this.ViewBag.ActivePage = MenuItem.DataExport;

            var statuses = new List<InterviewStatus?>
                {
                    InterviewStatus.InterviewerAssigned,
                    InterviewStatus.Completed,
                    InterviewStatus.ApprovedBySupervisor,
                    InterviewStatus.ApprovedByHeadquarters
                }
                .Select(item => new ComboboxViewItem{ Key = ((int)item.Value).ToString(), Value = item.ToUiString() })
                .ToArray();


            var export = new NewExportModel
            {
                Statuses = statuses,
                ExternalStoragesSettings = this.externalStoragesSettings is FakeExternalStoragesSettings
                    ? null
                    : this.externalStoragesSettings,
                Api = new
                {
                    // HistoryUrl = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "DataExportApi", action = "Paradata"}),
                    DDIUrl = Url.Action("DDIMetadata", "DataExportApi"),
                    ExportedDataReferencesForQuestionnaireUrl = Url.Action("GetExportStatus", "DataExportApi"),
                    UpdateSurveyDataUrl = Url.Action("RequestUpdate", "DataExportApi"),
                    RegenerateSurveyDataUrl = Url.Action("Regenerate", "DataExportApi"),
                    QuestionnairesUrl = Url.Action("QuestionnairesWithVersions", "QuestionnairesApi"),
                    StatusUrl = Url.Action("ExportStatus", "DataExportApi"),
                    ExportStatusUrl = Url.Action("Status", "DataExportApi"),
                    RunningJobsUrl = Url.Action("GetRunningJobs", "DataExportApi"),
                    DataAvailabilityUrl = Url.Action("DataAvailability", "DataExportApi"),
                    WasExportFileRecreatedUrl = Url.Action("WasExportFileRecreated", "DataExportApi"),
                    DownloadDataUrl = Url.Action("DownloadData", "DataExportApi"),
                    ExportToExternalStorageUrl = pathService.GetAbsolutePath(Url.Action("ExportToExternalStorage", "DataExportApi")),
                    CancelExportProcessUrl = Url.Action("DeleteDataExportProcess", "DataExportApi"),
                }
            };

            return this.View(export);
        }
    }
}
