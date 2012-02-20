using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
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
            var template = ((IComposite)group).Find<CompleteQuestion>(
                    bindedQuestion.ParentPublicKey);
            if (template == null)
            {
                IPropogate propagatebleGroup = group as IPropogate;
                if (propagatebleGroup == null)
                    return;
                var questionnaire = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc, null);
                template = questionnaire.Find<PropagatableCompleteGroup>(
                    g => g.PropogationPublicKey.Equals(propagatebleGroup.PropogationPublicKey)).SelectMany(
                        g => g.Find<CompleteQuestion>(q => q.PublicKey.Equals(bindedQuestion.ParentPublicKey))).
                    FirstOrDefault();
                if (template == null)
                {
                    throw new ArgumentException();
                }
            }
            this.Answers = template.Answers.Select(a => new CompleteAnswerView(a)).ToArray();
            this.PublicKey = template.PublicKey;
            this.QuestionText = template.QuestionText;
            this.QuestionType = template.QuestionType;
            this.ConditionExpression = template.ConditionExpression;
            this.StataExportCaption = template.StataExportCaption;
            this.GroupPublicKey = group.PublicKey;
           // thi's.QuestionnaireId = group.
        }
    }
}
