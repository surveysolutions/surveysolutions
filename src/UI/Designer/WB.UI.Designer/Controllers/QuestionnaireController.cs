// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireController.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Domain;
using WB.UI.Designer.Code;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.View;
    using Main.Core.View.Question;

    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Extensions;
    using WB.UI.Designer.Models;
    using WB.UI.Designer.Utils;
    using WB.UI.Designer.Views.Questionnaire;

    /// <summary>
    ///     The questionnaire controller.
    /// </summary>
    [CustomAuthorize]
    public class QuestionnaireController : BaseController
    {
        // GET: /Questionnaires/
        #region Constructors and Destructors

        public QuestionnaireController(IViewRepository repository, ICommandService commandService, IUserHelper userHelper)
            : base(repository, commandService,userHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clone.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Clone(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            return
                this.View(
                    new QuestionnaireCloneModel { Title = string.Format("{0}_Copy", model.Title), Id = model.PublicKey });
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView sourceModel = this.GetQuestionnaire(model.Id);
                if (sourceModel == null)
                {
                    throw new ArgumentNullException("model");
                }
                try
                {
                    this.CommandService.Execute(
                        new CloneQuestionnaireCommand(
                            Guid.NewGuid(), model.Title, UserHelper.CurrentUserId, sourceModel.Source));
                    return this.RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    if (e.InnerException is DomainException)
                    {
                        this.Error(e.InnerException.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return this.View(model);
        }

        /// <summary>
        ///     The create.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Create()
        {
            return this.View(new QuestionnaireViewModel());
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnaireViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                this.CommandService.Execute(
                    new CreateQuestionnaireCommand(Guid.NewGuid(), model.Title, UserHelper.CurrentUserId));
                return this.RedirectToActionPermanent("Index");
            }

            return View(model);
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            if ((model.CreatedBy != UserHelper.CurrentUserId) && !UserHelper.IsAdmin)
            {
                this.Error("You don't  have permissions to delete this questionnaire.");
            }
            else
            {
                this.CommandService.Execute(new DeleteQuestionnaireCommand(model.PublicKey));
                this.Success(string.Format("Questionnaire \"{0}\" successfully deleted", model.Title));
            }

            return this.Redirect(this.Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);

            if (model.CreatedBy != UserHelper.CurrentUserId)
            {
                throw new HttpException(403, string.Empty);
            }
            else
            {
                this.ReplaceGuidsInValidationAndConditionRules(model);
            }

            return View(model);
        }

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Export(Guid id)
        {
            return this.RedirectToAction("PreviewQuestionnaire", "Pdf", new { id });
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <summary>
        /// The public.
        /// </summary>
        /// <param name="p">
        /// The page index.
        /// </param>
        /// <param name="sb">
        /// The sort by.
        /// </param>
        /// <param name="so">
        /// The sort order.
        /// </param>
        /// <param name="f">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index(int? p, string sb, int? so, string f)
        {
            return this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f));
        }

        /// <summary>
        /// The public.
        /// </summary>
        /// <param name="p">
        /// The page index.
        /// </param>
        /// <param name="sb">
        /// The sort by.
        /// </param>
        /// <param name="so">
        /// The sort order.
        /// </param>
        /// <param name="f">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Public(int? p, string sb, int? so, string f)
        {
            return this.View(this.GetPublicQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get public questionnaires.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        private IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return QuestionnaireHelper.GetPublicQuestionnaires(
                repository: this.Repository, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter, 
                userId: UserHelper.CurrentUserId);
        }

        /// <summary>
        /// The get questionnaire by id.
        /// </summary>
        /// <param name="id">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// The <see cref="QuestionnaireView"/>.
        /// </returns>
        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire =
                this.Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpException(
                    (int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
            }

            return questionnaire;
        }

        /// <summary>
        /// The get items.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return QuestionnaireHelper.GetQuestionnaires(
                repository: this.Repository, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter, 
                userId: UserHelper.CurrentUserId);
        }

        /// <summary>
        /// The replace guids in validation and comdition rules.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        private void ReplaceGuidsInValidationAndConditionRules(QuestionnaireView model)
        {
            var transformator = new ExpressionReplacer(this.Repository);

            var elements = new Queue<ICompositeView>();

            foreach (ICompositeView compositeView in model.Children)
            {
                elements.Enqueue(compositeView);
            }

            while (elements.Count > 0)
            {
                ICompositeView element = elements.Dequeue();

                if (element is QuestionView)
                {
                    var question = (QuestionView)element;

                    question.ConditionExpression =
                        transformator.ReplaceGuidsWithStataCaptions(question.ConditionExpression, model.PublicKey);
                    question.ValidationExpression =
                        transformator.ReplaceGuidsWithStataCaptions(question.ValidationExpression, model.PublicKey);
                }

                if (element is GroupView)
                {
                    foreach (ICompositeView child in element.Children)
                    {
                        elements.Enqueue(child);
                    }
                }
            }
        }

        /// <summary>
        /// The save request.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string filter)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = filter;
            this.ViewBag.SortOrder = sortOrder;

            if (sortOrder.ToBool())
            {
                sortBy = string.Format("{0} Desc", sortBy);
            }
        }

        #endregion
    }
}