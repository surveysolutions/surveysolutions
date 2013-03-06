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

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Main.Core.Commands.Questionnaire.Completed;

    using Questionnaire.Core.Web.Helpers;

    /// <summary>
    /// Show Statistics
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        #region Fields

        /// <summary>
        /// ViewRepository object
        /// </summary>
        private IViewRepository viewRepository;

        /// <summary>
        /// The _global provider.
        /// </summary>
        private readonly IGlobalInfoProvider _globalProvider;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public DashboardController(IViewRepository viewRepository, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Show all template for questionnaire
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Return page with template questionnaire
        /// </returns>
        public ActionResult Questionnaires(QuestionnaireBrowseInputModel input)
        {
             var model = this.viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
             return this.View(model);
        }
  
        /// <summary>
        /// Dispay page for creation new questionnaire
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Redirect to form creation new questionnaire
        /// </returns>
        /// <exception cref="HttpException">
        /// throw 404 exception
        /// </exception>
        public ActionResult NewSurvey(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key, this._globalProvider.GetCurrentUser()));
            return this.RedirectToAction("Assign", "Survey", new { Id = newQuestionnairePublicKey, Template = id });
        }
        #endregion
    }
}
