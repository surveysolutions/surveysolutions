using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.WCF;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Synchronization;

namespace RavenQuestionnaire.Web.Controllers
{
    public class SynchronizationsController : AsyncController
    {
        private readonly IBagManager _bagManager;
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public SynchronizationsController(ICommandInvoker commandInvoker, IViewRepository viewRepository,
                                          IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            _bagManager = bagManager;
            _globalProvider = globalProvider;
        }

        public ActionResult Index(string url)
        {
            //    AsyncManager.OutstandingOperations.Increment();
            UserLight user = _globalProvider.GetCurrentUser();

            Guid syncProcess = Guid.NewGuid();
            commandInvoker.Execute(
                new CreateNewSynchronizationProcessCommand(syncProcess, user));
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments = url + " " + syncProcess;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = WebConfigurationManager.AppSettings["SynchronizerPath"];
            p.Start();
            return RedirectToAction("Progress", new {id = syncProcess});
        }

        public ActionResult Progress(Guid id)
        {
            return View(viewRepository.Load<SyncProgressInputModel, SyncProgressView>(new SyncProgressInputModel(id)));
        }

        public ActionResult ProgressPartial(Guid id)
        {
            return PartialView("_ProgressContent",
                               viewRepository.Load<SyncProgressInputModel, SyncProgressView>(
                                   new SyncProgressInputModel(id)));
        }

        public ActionResult DiscoverPage()
        {
            return View("Scaning");
        }

        public void DiscoverAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            UserLight user = _globalProvider.GetCurrentUser();
            AsyncQuestionnaireUpdater.Update(() =>
                                                 {
                                                     try
                                                     {
                                                         AsyncManager.Parameters["result"] =
                                                             new ServiceDiscover().DiscoverChannels();
                                                     }
                                                     catch
                                                     {
                                                         AsyncManager.Parameters["result"] = null;
                                                     }
                                                     AsyncManager.OutstandingOperations.Decrement();
                                                 });
        }

        public ActionResult DiscoverCompleted(IEnumerable<ServiceDiscover.SyncSpot> result)
        {
            return PartialView("Spots", result.ToArray());
        }
    }
}