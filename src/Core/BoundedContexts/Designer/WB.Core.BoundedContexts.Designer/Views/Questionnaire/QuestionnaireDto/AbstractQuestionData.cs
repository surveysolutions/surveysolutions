using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class AbstractQuestionData : QuestionnaireActive
    {
        protected AbstractQuestionData()
        {
        }

        public AbstractQuestionData(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, QuestionProperties properties, bool capital, Guid publicKey, string questionText, QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, IList<ValidationCondition> validationConditions) : base(responsibleId)
        {
            this.ConditionExpression = conditionExpression;
            this.HideIfDisabled = hideIfDisabled;
            this.Featured = featured;
            this.Instructions = instructions;
            this.Properties = properties;
            this.Capital = capital;
            this.PublicKey = publicKey;
            this.QuestionText = questionText;
            this.QuestionScope = questionScope;
            this.StataExportCaption = stataExportCaption;
            this.VariableLabel = variableLabel;

            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
            this.ValidationConditions = this.ValidationConditions.ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage);
        }

        public string ConditionExpression { get; private set; }
        public bool HideIfDisabled { get; private set; }
        public bool Featured { get; private set; }
        public string Instructions { get; private set; }
        public QuestionProperties Properties { get; set; }
        public bool Capital { get; private set; }
        public Guid PublicKey { get; private set; }
        public string QuestionText { get; private set; }
        public QuestionScope QuestionScope { get; private set; }
        public string StataExportCaption { get; private set; }
        public string VariableLabel { get; private set; }
        [Obsolete("KP-6647")]
        public string ValidationExpression { get; private set; }
        [Obsolete("KP-6647")]
        public string ValidationMessage { get; private set; }

        public IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
