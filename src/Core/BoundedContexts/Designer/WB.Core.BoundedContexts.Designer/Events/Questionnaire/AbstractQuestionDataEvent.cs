using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractQuestionDataEvent : QuestionnaireActiveEvent
    {
        protected AbstractQuestionDataEvent()
        {
        }

        public AbstractQuestionDataEvent(Guid responsibleId, string conditionExpression, bool featured, string instructions, bool capital, Guid publicKey, string questionText, QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage) : base(responsibleId)
        {
            this.ConditionExpression = conditionExpression;
            this.Featured = featured;
            this.Instructions = instructions;
            this.Capital = capital;
            this.PublicKey = publicKey;
            this.QuestionText = questionText;
            this.QuestionScope = questionScope;
            this.StataExportCaption = stataExportCaption;
            this.VariableLabel = variableLabel;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;

            this.ValidationConditions = new List<ValidationCondition>();
            if (!string.IsNullOrEmpty(this.ValidationExpression))
            {
                this.ValidationConditions.Add(new ValidationCondition
                {
                    Expression = this.ValidationExpression,
                    Message = this.ValidationMessage
                });
            }
        }

        public string ConditionExpression { get; private set; }
        public bool Featured { get; private set; }
        public string Instructions { get; private set; }
        public bool Capital { get; private set; }
        public Guid PublicKey { get; private set; }
        public string QuestionText { get; private set; }
        public QuestionScope QuestionScope { get; private set; }
        public string StataExportCaption { get; private set; }
        public string VariableLabel { get; private set; }
        public string ValidationExpression { get; private set; }
        public string ValidationMessage { get; private set; }

        public List<ValidationCondition> ValidationConditions { get; set; }
    }
}
