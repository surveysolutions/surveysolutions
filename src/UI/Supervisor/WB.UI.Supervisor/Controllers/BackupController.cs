using System;
using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.Models;

namespace WB.UI.Supervisor.Controllers
{
    public class BackupController : AsyncController
    {
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Administration;
            return this.View();
        }

        public string CreateBackupAsync(Guid syncKey)
        {
            return "Started";
        }

        public ActionResult CreateBackupCompleted()
        {
            return null;
        }
    }
}