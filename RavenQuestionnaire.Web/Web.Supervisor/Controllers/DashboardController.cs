// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the DashboardController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.Questionnaire;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Ncqrs.Commanding.ServiceModel;
    using Main.Core.Commands.Questionnaire.Completed;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;

    /// <summary>
    /// Show Statistics
    /// </summary>
    [Authorize(Roles = "Headquarter")]
    public class DashboardController : BaseController
    { 
        public DashboardController(
            IViewRepository viewRepository, ICommandService commandService, IGlobalInfoProvider globalProvider, ILog logger)
            : base(viewRepository, commandService, globalProvider, logger)
        {
        }
      
        public ActionResult Questionnaires(QuestionnaireBrowseInputModel input)
        {
            ViewBag.ActivePage = MenuItem.Administration;
             var model = this.Repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
             return this.View(model);
        }
      
        public ActionResult NewSurvey(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key, this.GlobalInfo.GetCurrentUser()));
            return this.RedirectToAction("Assign", "HQ", new { Id = newQuestionnairePublicKey, Template = id });
        }
    }
}
