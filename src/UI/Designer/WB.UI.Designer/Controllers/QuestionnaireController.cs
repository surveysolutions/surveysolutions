// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireController.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Documents;
    using Main.Core.View;
    using Main.Core.View.Question;
    using Main.Core.View.Questionnaire;

    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Code.Exceptions;
    using WB.UI.Designer.Models;
    using WB.UI.Designer.Utils;
    using WB.UI.Designer.Views.Questionnaire;

    /// <summary>
    ///     The questionnaire controller.
    /// </summary>
    [Authorize]
    public class QuestionnaireController : AlertController
    {
        // GET: /Questionnaires/
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        public QuestionnaireController(IViewRepository repository, ICommandService commandService)
            : base(repository, commandService)
        {
        }

        #endregion

        #region Public Methods and Operators

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
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            if ((model.CreatedBy != UserHelper.CurrentUserId) && !UserHelper.IsAdmin)
            {
                throw new DesignerPermissionException();
            }

            return this.View(new DeleteQuestionnaireModel { Id = model.PublicKey, Title = model.Title });
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
        public ActionResult DeleteConfirmed(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            if ((model.CreatedBy != UserHelper.CurrentUserId) && !UserHelper.IsAdmin)
            {
                throw new DesignerPermissionException();
            }

            this.CommandService.Execute(new DeleteQuestionnaireCommand(model.PublicKey));

            return this.RedirectToAction("Index");
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
                throw new DesignerPermissionException();
            }

            this.ReplaceGuidsInValidationAndConditionRules(model);

            return View(model);
        }

        public ActionResult Export(Guid id)
        {
            return this.RedirectToAction("PreviewQuestionnaire", "Pdf", new { id = id });
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
        public ActionResult Index(int? p, string sb, bool? so, string f)
        {
            return this.View(this.GetItems(true, p, sb, so, f));
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
        public ActionResult Public(int? p, string sb, bool? so, string f)
        {
            return this.View(this.GetItems(false, p, sb, so, f));
        }

        #endregion

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
            var model = this.GetQuestionnaire(id);
            return
                this.View(
                    new QuestionnaireCloneModel()
                        {
                            Title = string.Format("{0}_Copy", model.Title),
                            Id = model.PublicKey
                        });
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
            if (ModelState.IsValid)
            {
                var sourceModel = this.GetQuestionnaire(model.Id);
                if (sourceModel == null)
                {
                    throw new ArgumentNullException("model");
                }

                CommandService.Execute(
                    new CloneQuestionnaireCommand(
                        Guid.NewGuid(), model.Title, UserHelper.CurrentUserId, sourceModel.Source));
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        #region Methods

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

            return questionnaire;
        }

        /// <summary>
        /// The get items.
        /// </summary>
        /// <param name="isOnlyOwnerItems">
        /// The is only owner items.
        /// </param>
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
        private IPagedList<QuestionnaireListViewModel> GetItems(
            bool isOnlyOwnerItems, int? pageIndex, string sortBy, bool? sortOrder, string filter)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = filter;
            this.ViewBag.SortOrder = sortOrder;
            if (this.ViewBag.SortOrder != null && this.ViewBag.SortOrder)
            {
                sortBy = string.Format("{0} Desc", sortBy);
            }

            QuestionnaireBrowseView model =
                this.Repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                    input:
                        new QuestionnaireBrowseInputModel
                            {
                                CreatedBy = UserHelper.CurrentUserId, 
                                IsOnlyOwnerItems = isOnlyOwnerItems, 
                                IsAdminMode = UserHelper.IsAdmin, 
                                Page = pageIndex ?? 1, 
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
            IPagedList<QuestionnaireListViewModel> retVal =
                model.Items.Select(
                    x =>
                    new QuestionnaireListViewModel
                        {
                            Id = x.Id, 
                            CreationDate = x.CreationDate, 
                            LastEntryDate = x.LastEntryDate, 
                            Title = x.Title, 
                            CanDelete = isOnlyOwnerItems || UserHelper.IsAdmin, 
                            CanEdit = isOnlyOwnerItems
                        })
                     .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);

            return retVal;
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

            foreach (var compositeView in model.Children)
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

        #endregion
    }
}