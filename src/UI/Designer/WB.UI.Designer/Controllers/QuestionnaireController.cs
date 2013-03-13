using Main.Core.Commands.Questionnaire;
using Main.Core.View;
using Main.Core.View.Question;
using Main.Core.View.Questionnaire;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WB.UI.Designer.Models;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    using System.Web.Security;

    [Authorize]
    public class QuestionnaireController : BootstrapBaseController
    {
        //
        // GET: /Questionnaires/

        public QuestionnaireController(IViewRepository repository, ICommandService commandService)
            : base(repository, commandService)
        {

        }

        public ActionResult Index()
        {
            return this.View(this.GetItems(true));
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

        public ActionResult Public()
        {
            return this.View(this.GetItems(false));
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

        private IEnumerable<QuestionnaireListViewModel> GetItems(bool isOnlyOwnerItems)
        {
            IEnumerable<QuestionnaireListViewModel> retVal = default(IEnumerable<QuestionnaireListViewModel>);

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
                                            user.UserName, UserHelper.ADMINROLENAME)
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
                            }).ToArray();
            }
            return retVal;
        }
    }
}
