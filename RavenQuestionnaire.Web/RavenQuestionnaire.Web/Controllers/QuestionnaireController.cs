// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Utility;
using Main.Core.View.Question;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Web.Utils;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.Questionnaire;
    using Main.Core.Export;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Security;
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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public QuestionnaireController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
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

            QuestionnaireView model = this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

            ReplaceGuidsInValidationAndComditionRules(id, model);

            return View(model);
        }

        private void ReplaceGuidsInValidationAndComditionRules(Guid id, QuestionnaireView model)
        {
            var transformator = new ExpressionReplacer(this.viewRepository);

            var elements = new Queue<ICompositeView>(model.Children.OfType<GroupView>());

            while (elements.Count > 0)
            {
                ICompositeView element = elements.Dequeue();

                if (element is QuestionView)
                {
                    var question = (QuestionView) element;
                    question.ConditionExpression = transformator.ReplaceGuidsWithStataCaptions(question.ConditionExpression, id);
                    question.ValidationExpression = transformator.ReplaceGuidsWithStataCaptions(question.ValidationExpression, id);
                }

                if (element is GroupView)
                {
                    foreach (var child in element.Children)
                    {
                        elements.Enqueue(child);
                    }
                }
            }

#warning TLK: deal with commented code here

//            var treeStack = new Stack<GroupView>();
//            foreach (var group in model.Children.OfType<GroupView>())
//            {
//                treeStack.Push(@group);
//            }
//
//            while (treeStack.Count > 0)
//            {
//                var group = treeStack.Pop();
//
//                foreach (var question in @group.Children.OfType<QuestionView>())
//                {
//                    question.ConditionExpression = transformator.ReplaceGuidsWithStataCaptions(question.ConditionExpression, id);
//                    question.ValidationExpression = transformator.ReplaceGuidsWithStataCaptions(question.ValidationExpression,
//                                                                                                id);
//                }
//
//                foreach (var g in @group.Children.OfType<GroupView>())
//                {
//                    treeStack.Push(@group);
//                }
//            }
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