using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteQuestion : AbstractCompleteQuestion, IPropogate
    {
        public PropagatableCompleteQuestion()
        {
        }

        public PropagatableCompleteQuestion(ICompleteQuestion question, Guid propogationPublicKey)
        {
            this.ConditionExpression = question.ConditionExpression;
            this.Enabled = question.Enabled;
            this.PublicKey = question.PublicKey;

            this.QuestionText = question.QuestionText;
            this.QuestionType = question.QuestionType;
            this.Triggers = question.Triggers;
            this.StataExportCaption = question.StataExportCaption;
            this.Featured = question.Featured;

            for (int i = 0; i < question.Children.Count; i++)
            {
                this.Children.Add(new PropagatableCompleteAnswer(question.Children[i] as ICompleteAnswer, propogationPublicKey));
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

        public bool AutoPropagate { get; set; }

        #endregion
    }
}
