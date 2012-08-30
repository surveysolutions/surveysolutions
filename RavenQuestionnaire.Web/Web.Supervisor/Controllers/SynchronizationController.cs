using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Events;

namespace Web.Supervisor.Controllers
{

    public class SynchronizationController : Controller
    {
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;
        private readonly IExportImport exportimportEvents ;
        private readonly IEventSync synchronizer;

        public SynchronizationController( IViewRepository viewRepository,
                                          IGlobalInfoProvider globalProvider, IExportImport exportImport, IEventSync synchronizer)
        {
            this.exportimportEvents = exportImport;
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
            this.synchronizer = synchronizer;
        }
    }
}
