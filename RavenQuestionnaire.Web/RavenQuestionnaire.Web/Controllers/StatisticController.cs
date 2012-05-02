using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Web.Controllers
{
    public class StatisticController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
     //   private IBagManager _bagManager;
        private IGlobalInfoProvider _globalProvider;

        public StatisticController(ICommandInvoker commandInvoker, IViewRepository viewRepository,IGlobalInfoProvider globalProvider)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
          //  this._bagManager = bagManager;
            this._globalProvider = globalProvider;
        }
    /*    public void Genereate(string id)
        {
            var command = new GenerateQuestionnaireStatisticCommand(id, _globalProvider.GetCurrentUser());

            commandInvoker.Execute(command);
        }*/
        public ActionResult Details(string id)
        {
        /*    var command = new GenerateQuestionnaireStatisticCommand(id, _globalProvider.GetCurrentUser());

            commandInvoker.Execute(command);*/
            var stat =viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                new CompleteQuestionnaireStatisticViewInputModel(id));
            return PartialView(stat);
        }
    }
}
