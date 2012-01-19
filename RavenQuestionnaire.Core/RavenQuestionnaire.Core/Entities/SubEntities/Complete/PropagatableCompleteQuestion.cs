using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteQuestion : CompleteQuestion, IPropogate
    {
        public PropagatableCompleteQuestion()
        {
        }

        public PropagatableCompleteQuestion(CompleteQuestion question, Guid propogationPublicKey)
        {
            this.ConditionExpression = question.ConditionExpression;
            this.Enabled = question.Enabled;
            this.PublicKey = question.PublicKey;
            this.QuestionText = question.QuestionText;
            this.QuestionType = question.QuestionType;
            this.StataExportCaption = question.StataExportCaption;

            for (int i = 0; i < question.Answers.Count; i++)
            {
                this.Answers.Add(new PropagatableCompleteAnswer(question.Answers[i], propogationPublicKey));
            }

            this.PropogationPublicKey = propogationPublicKey;
        }

        #region Implementation of ICloneable

        public object Clone()
        {
            throw new InvalidOperationException("answer can't propagated");
        }

        #endregion

        #region Implementation of IPropogate

        public Guid PropogationPublicKey { get; set; }

        #endregion
    }
}
