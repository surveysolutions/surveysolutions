using System;
using System.Collections.Generic;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireEnumerableViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireEnumerableViewFactory(IDocumentSession documentSession, IExpressionExecutor<Entities.CompleteQuestionnaire> executor)
        {
            this.documentSession = documentSession;
            this.conditionExecutor = executor;
        }
        private IExpressionExecutor<Entities.CompleteQuestionnaire> conditionExecutor;
        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                RavenQuestionnaire.Core.Entities.SubEntities.Group group = null;

                Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Group, Guid> iterator =
                       new QuestionnaireScreenIterator(completeQuestionnaireRoot);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    var template = new Entities.Questionnaire(doc.Questionnaire);
                    group =
                        template.Find<RavenQuestionnaire.Core.Entities.SubEntities.Group>(
                            input.CurrentGroupPublicKey.Value);
                }
                else if (input.PreviousGroupPublicKey.HasValue)
                {
                   
                    group = input.IsReverse
                                ? iterator.GetPreviousBefoure(input.PreviousGroupPublicKey.Value)
                                : iterator.GetNextAfter(input.PreviousGroupPublicKey.Value);
                }
                else
                {
                    group = input.IsReverse ? iterator.Last : iterator.First;
                }
                return new CompleteQuestionnaireViewEnumerable(doc,
                                                               new CompleteGroupView(doc, group,
                                                                                     ProcessQuestionList(completeQuestionnaireRoot,
                                                                                                         group.Questions)));
            }
            if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireViewEnumerable(doc);
            }
            return null;

        }

        protected CompleteQuestionView[] ProcessQuestionList(Entities.CompleteQuestionnaire entity,
            IList<RavenQuestionnaire.Core.Entities.SubEntities.Question> questions)
        {
            CompleteQuestionView[] result = new CompleteQuestionView[questions.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CompleteQuestionView(questions[i], ((IEntity<QuestionnaireDocument>)entity.GetQuestionnaireTemplate()).GetInnerDocument());
                result[i].Enabled = this.conditionExecutor.Execute(entity, questions[i].ConditionExpression);
           //     RemoveDisabledAnswers(this.completeQuestionnaireDocument.CompletedAnswers, result[i]);
            }
            return result;
        }
    }
}
