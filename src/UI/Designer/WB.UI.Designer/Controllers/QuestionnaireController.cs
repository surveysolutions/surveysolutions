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
            QuestionnaireBrowseView model = Repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                   new QuestionnaireBrowseInputModel());
            return View(model.Items);
        }

        public ActionResult Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            QuestionnaireView model = Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

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
                CommandService.Execute(new CreateQuestionnaireCommand(Guid.NewGuid(), model.Title));
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
        }

        public ActionResult Public()
        {
            return RedirectToActionPermanent("Index");
        }
    }
}
