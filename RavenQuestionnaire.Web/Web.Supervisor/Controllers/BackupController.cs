namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Web.Supervisor.Models;

    /// <summary>
    /// The backup controller.
    /// </summary>
    public class BackupController : AsyncController
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.ActivePage = MenuItem.Administration;
            return View();
        }

        /// <summary>
        /// The create backup async.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CreateBackupAsync(Guid syncKey)
        {
            return "Started";
        }

        /// <summary>
        /// The create backup completed.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult CreateBackupCompleted ()
        {
            return null;
        }
    }
}
