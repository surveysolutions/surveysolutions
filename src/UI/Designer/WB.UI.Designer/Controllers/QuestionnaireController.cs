using Main.Core.Commands.Questionnaire;
using Main.Core.View;
using Main.Core.View.Question;
using Main.Core.View.Questionnaire;
using Ncqrs.Commanding.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WB.UI.Designer.Models;
using WB.UI.Designer.Utils;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Controllers
{
    using System.Web.Security;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;

    [Authorize]
    public class QuestionnaireController : AlertController
    {
        //
        // GET: /Questionnaires/

        public QuestionnaireController(IViewRepository repository, ICommandService commandService)
            : base(repository, commandService)
        {

        }

        public ActionResult Index(int? p, string sb, bool? so, string f)
        {
            return this.View(this.GetItems(true, p, sb, so, f));
        }

        public ActionResult Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            QuestionnaireView model =
                Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

            ReplaceGuidsInValidationAndComditionRules(id, model);

            return View(model);
        }

        public ActionResult Create()
        {
            return View(new QuestionnaireViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnaireViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Membership.GetUser(User.Identity.Name);
                CommandService.Execute(
                    new CreateQuestionnaireCommand(
                        Guid.NewGuid(), model.Title, user != null ? (Guid?)user.ProviderUserKey : null));
                return RedirectToActionPermanent("Index");
            }
            return View(model);
        }

        private void ReplaceGuidsInValidationAndComditionRules(Guid id, QuestionnaireView model)
        {
            var transformator = new ExpressionReplacer(Repository);

            var elements = new Queue<ICompositeView>(model.Children.OfType<GroupView>());

            while (elements.Count > 0)
            {
                ICompositeView element = elements.Dequeue();

                if (element is QuestionView)
                {
                    var question = (QuestionView)element;
                    question.ConditionExpression =
                        transformator.ReplaceGuidsWithStataCaptions(question.ConditionExpression, id);
                    question.ValidationExpression =
                        transformator.ReplaceGuidsWithStataCaptions(question.ValidationExpression, id);
                }

                if (element is GroupView)
                {
                    foreach (var child in element.Children)
                    {
                        elements.Enqueue(child);
                    }
                }
            }
        }

        public ActionResult Public(int? p, string sb, bool? so, string f)
        {
            return this.View(this.GetItems(false, p, sb, so, f));
        }

        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model =
                Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View(new DeleteQuestionnaireModel() { Id = model.PublicKey, Title = model.Title });
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            CommandService.Execute(new DeleteQuestionnaireCommand(id));

            return RedirectToAction("Index");
        }

        private IPagedList<QuestionnaireListViewModel> GetItems(
            bool isOnlyOwnerItems, int? pageIndex, string sortBy, bool? sortOrder, string filter)
        {
            ViewBag.PageIndex = pageIndex;
            ViewBag.SortBy = sortBy;
            ViewBag.Filter = filter;
            ViewBag.SortOrder = sortOrder;
            if (ViewBag.SortOrder != null && ViewBag.SortOrder)
            {
                sortBy = string.Format("{0} Desc", sortBy);
            }
            
            IPagedList<QuestionnaireListViewModel> retVal = default(IPagedList<QuestionnaireListViewModel>);

            var user = Membership.GetUser(User.Identity.Name);
            if (user != null)
            {
                QuestionnaireBrowseView model =
                    this.Repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                        input:
                            new QuestionnaireBrowseInputModel()
                                {
                                    CreatedBy = (Guid)user.ProviderUserKey,
                                    IsOnlyOwnerItems = isOnlyOwnerItems,
                                    IsAdminMode =
                                        Roles.IsUserInRole(
                                            user.UserName, UserHelper.ADMINROLENAME),
                                    Page = pageIndex ?? 1,
                                    PageSize = GlobalHelper.GridPageItemsCount,
                                    Order = sortBy, 
                                    Filter = filter
                                });
                retVal =
                    model.Items.Select(
                        x =>
                        new QuestionnaireListViewModel()
                            {
                                Id = x.Id,
                                CreationDate = x.CreationDate,
                                LastEntryDate = x.LastEntryDate,
                                Title = x.Title
                            })
                         .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
            }
            return retVal;
        }
    }
}
