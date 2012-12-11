// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Export;
using Questionnaire.Core.Web.Export;
using RavenQuestionnaire.Web.Export;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.Question;
    using Main.Core.View.Questionnaire;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Security;
    using RavenQuestionnaire.Core.Views.Group;
    using RavenQuestionnaire.Core.Views.Questionnaire;
    using RavenQuestionnaire.Web.Models;

    /// <summary>
    /// The questionnaire controller.
    /// </summary>
    [Authorize]
    public class QuestionnaireController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        private readonly IDataExport exporter;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public QuestionnaireController(IViewRepository viewRepository, IDataExport exporter)
        {
            this.viewRepository = viewRepository;
            this.exporter = exporter;
        }

        #endregion

        // GET: /Questionnaire/Create
        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor)]
        public ActionResult Create()
        {
            return this.View(new QuestionnaireView());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public ViewResult Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

            QuestionnaireView model =
                this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));

            return View(model);
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public ViewResult Flow(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

            QuestionnaireView model =
                this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));

            return View(model);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters.");
            }

            QuestionnaireView model =
                this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));
            return View("Create", model);
        }

        // POST: /Questionnaire/Create

         /*
        //
        // GET: /Questionnaire/Delete/5
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteQuestionnaireCommand(id, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }*/

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Export(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

            QuestionnaireView model =
                this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));
            return View(model);
        }

        /// <summary>
        /// The get exported data.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public FileResult GetExportedData(Guid id, string type)
        {
            if ((id == null) || (id == Guid.Empty) || string.IsNullOrEmpty(type))
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

          /*  QuestionnaireView model =
                this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));

            if (model != null)
            {*/

            return this.File(this.exporter.ExportData(id, type), "application/zip", "data.zip");
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult Index()
        {
            QuestionnaireBrowseView model =
                this.viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                    new QuestionnaireBrowseInputModel());
            return View(model);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionnaireView model)
        {
            if (this.ModelState.IsValid)
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                if (model.PublicKey == Guid.Empty)
                {
                    // maybe better move loading defaults to the handler?
                    Guid key = Guid.NewGuid();

                    // new fw
                    commandService.Execute(new CreateQuestionnaireCommand(key, model.Title));
                }
                else
                {
                    commandService.Execute(new UpdateQuestionnaireCommand(model.PublicKey, model.Title));
                }

                return this.RedirectToAction("Index");
            }

            return View("Create", model);
        }

        /// <summary>
        /// The _ table data.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableData(GridDataRequest data)
        {
            var input = new QuestionnaireBrowseInputModel
                {
                   Page = data.Pager.Page, PageSize = data.Pager.PageSize, Orders = data.SortOrder 
                };
            QuestionnaireBrowseView model =
                this.viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return this.PartialView("_Table", model);
        }

        #endregion
    }
}