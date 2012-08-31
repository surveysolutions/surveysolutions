using System.Linq;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Events;
using Questionnaire.Core.Web.Threading;

namespace Web.Supervisor.Controllers
{
    
    [AsyncTimeout(20000000)]
    public class SynchronizationController : AsyncController
    {

        #region Properties

        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;
        private readonly IExportImport exportimportEvents ;
        private readonly IEventSync synchronizer;

        #endregion

        #region Constructor

        public SynchronizationController( IViewRepository viewRepository,
                                          IGlobalInfoProvider globalProvider, IExportImport exportImport, IEventSync synchronizer)
        {
            this.exportimportEvents = exportImport;
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
            this.synchronizer = synchronizer;
        }

        #endregion

        #region CAPI

        public bool CheckIsThereSomethingToPush()
        {
            return this.synchronizer.ReadEvents().Any();
        }

        public bool CheckNoCompletedQuestionnaires()
        {
            return this.synchronizer.ReadEvents().Any();
        }

        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile == null && Request.Files.Count > 0)
                myfile = Request.Files[0];
            if (myfile != null && myfile.ContentLength != 0)
            {
                AsyncManager.OutstandingOperations.Increment();
                AsyncQuestionnaireUpdater.Update(() =>
                {
                    exportimportEvents.Import(myfile);
                    AsyncManager.OutstandingOperations.Decrement();
                });
            }
        }

        public ActionResult ImportCompleted()
        {
            return RedirectToAction("Index", "Dashboard");
        }
       

        #endregion
    }
}
