using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Web.Models;

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

        public void IndexAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            var user = _globalProvider.GetCurrentUser();
            AsyncQuestionnaireUpdater.Update(() =>
                                                 {
                                                     Process p = new Process();
                                                     p.StartInfo.UseShellExecute = false;
                                                     p.StartInfo.RedirectStandardOutput = true;
                                                     p.StartInfo.FileName = System.Web.Configuration.WebConfigurationManager.AppSettings["SynchronizerPath"];
                                                     try
                                                     {
                                                         p.Start();
                                                         p.WaitForExit();
                                                         int result = p.ExitCode;
                                                         AsyncManager.Parameters["result"] = result == 1;
                                                        
                                                     }
                                                     catch
                                                     {
                                                         AsyncManager.Parameters["result"] = false;
                                                     }
                                                     AsyncManager.OutstandingOperations.Decrement();
                                                 });
        }

        public bool IndexCompleted(bool result)
        {
           
            return result;
        }

    }
}
