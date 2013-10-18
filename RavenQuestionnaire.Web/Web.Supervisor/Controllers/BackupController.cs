using System;
using System.Web.Mvc;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
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