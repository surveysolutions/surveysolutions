// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticController.cs" company="">
//   
// </copyright>
// <summary>
//   The statistic controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire.Statistics;

namespace RavenQuestionnaire.Web.Controllers
{
    using System.Web.Mvc;

    using Questionnaire.Core.Web.Helpers;

    using RavenQuestionnaire.Core;

    /// <summary>
    /// The statistic controller.
    /// </summary>
    public class StatisticController : Controller
    {
        #region Fields

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        // private IBagManager _bagManager;
        /// <summary>
        /// The _global provider.
        /// </summary>
        private IGlobalInfoProvider globalProvider;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="globalProvider">
        /// The global provider.
        /// </param>
        public StatisticController(IViewRepository viewRepository, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = viewRepository;

            // this._bagManager = bagManager;
            this.globalProvider = globalProvider;
        }

        #endregion

        /*    public void Genereate(string id)
        {
            var command = new GenerateQuestionnaireStatisticCommand(id, _globalProvider.GetCurrentUser());

            commandInvoker.Execute(command);
        }*/
        #region Public Methods and Operators

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        public ActionResult Details(string id)
        {
            /*    var command = new GenerateQuestionnaireStatisticCommand(id, _globalProvider.GetCurrentUser());

            commandInvoker.Execute(command);*/
            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id));
            return PartialView(stat);
        }

        #endregion
    }
}