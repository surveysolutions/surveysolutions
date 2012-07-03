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
                doc.Find<AbstractCompleteQuestion>(
                    q => q.PublicKey ==
                         bindedQuestion.ParentPublicKey);
            var template = templates.FirstOrDefault();
            if (templates.Count() > 1)
            {
                //IPropogate propagatebleGroup = group as IPropogate;
                if (!group.PropogationPublicKey.HasValue)
                    return;
              //  var questionnaire = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc);
                template = doc.Find<ICompleteGroup>(
                    g => g.PropogationPublicKey == group.PropogationPublicKey.Value).SelectMany(
                        g => g.Find<AbstractCompleteQuestion>(q => q.PublicKey.Equals(bindedQuestion.ParentPublicKey))).
                    FirstOrDefault();
                if (template == null)
                {
                    return;
                }
            }
            this.Answers = template.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(template.PublicKey,a)).ToArray();
            this.PublicKey = template.PublicKey;
            this.Title = template.QuestionText;
            this.Instructions = template.Instructions;
            this.Comments = template.Comments;
            this.ConditionExpression = template.ConditionExpression;
            this.StataExportCaption = template.StataExportCaption;
            this.Parent = group.PublicKey;
           // thi's.QuestionnaireId = group.
        }
    }
}
