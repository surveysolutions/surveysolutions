using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class BindedCompleteQuestionView : CompleteQuestionView
    {
        public BindedCompleteQuestionView(
            CompleteQuestionnaireDocument questionnaire, ICompleteQuestion question)
        {
          
            this.Enabled = false;
            var bindedQuestion = question as IBinded;
            if (bindedQuestion == null)
            {
                throw new ArgumentException();
            }
            var template =
                new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(questionnaire, null).Find<CompleteQuestion>(
                    bindedQuestion.ParentPublicKey);
            this.Answers = template.Answers.Select(a => new CompleteAnswerView(a)).ToArray();
            this.PublicKey = template.PublicKey;
            this.QuestionText = template.QuestionText;
            this.QuestionType = template.QuestionType;
            this.ConditionExpression = template.ConditionExpression;
            this.StataExportCaption = template.StataExportCaption;
            this.GroupPublicKey = GetQuestionGroup(questionnaire, template.PublicKey);
            this.QuestionnaireId = questionnaire.Id;
        }
    }
}
