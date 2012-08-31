using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Events;

namespace Web.Supervisor.Controllers
{
    using System.Collections.Generic;

    public class SynchronizationController : Controller
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

        public List<AggregateRootEvent> SelectNoCompletedQuestionnaire()
        {
            return this.synchronizer.ReadEvents().ToList();
        }

        #endregion
    }
}
