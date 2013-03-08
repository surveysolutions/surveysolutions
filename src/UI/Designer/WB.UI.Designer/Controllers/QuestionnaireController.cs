using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using Main.Core.View.Question;
using Main.Core.View.Questionnaire;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class QuestionnaireController : BootstrapBaseController
    {
        //
        // GET: /Questionnaires/

        public QuestionnaireController(IViewRepository repository) : base(repository)
        {
        }

        public ActionResult Index()
        {
            QuestionnaireBrowseView model = this._repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                   new QuestionnaireBrowseInputModel());
            return View(model.Items);
        }

        public ActionResult Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            QuestionnaireView model = this._repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

            ReplaceGuidsInValidationAndComditionRules(id, model);

            return View(model);
        }

        public ActionResult Create()
        {
            return View(new QuestionnaireView());
        }

        private void ReplaceGuidsInValidationAndComditionRules(Guid id, QuestionnaireView model)
        {
            var transformator = new ExpressionReplacer(this._repository);

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
