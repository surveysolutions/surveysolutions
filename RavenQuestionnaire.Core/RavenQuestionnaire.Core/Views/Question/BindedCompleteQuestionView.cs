using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class BindedCompleteQuestionView : CompleteQuestionView
    {
        public BindedCompleteQuestionView(CompleteQuestionnaireDocument doc,
            ICompleteGroup group, ICompleteQuestion question)
        {
          
            this.Enabled = false;
            var bindedQuestion = question as IBinded;
            if (bindedQuestion == null)
            {
                throw new ArgumentException();
            }
            var templates =
                new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc).Find<CompleteQuestion>(
                    q => q.PublicKey ==
                         bindedQuestion.ParentPublicKey);
            var template = templates.FirstOrDefault();
            if (templates.Count() > 1)
            {
                IPropogate propagatebleGroup = group as IPropogate;
                if (propagatebleGroup == null)
                    return;
                var questionnaire = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc);
                template = questionnaire.Find<PropagatableCompleteGroup>(
                    g => g.PropogationPublicKey.Equals(propagatebleGroup.PropogationPublicKey)).SelectMany(
                        g => g.Find<CompleteQuestion>(q => q.PublicKey.Equals(bindedQuestion.ParentPublicKey))).
                    FirstOrDefault();
                if (template == null)
                {
                    return;
                }
            }
            this.Answers = template.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(a)).ToArray();
            this.PublicKey = template.PublicKey;
            this.QuestionText = template.QuestionText;
            this.Instructions = template.Instructions;
            this.ConditionExpression = template.ConditionExpression;
            this.StataExportCaption = template.StataExportCaption;
            this.GroupPublicKey = group.PublicKey;
           // thi's.QuestionnaireId = group.
        }
    }
}
