using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class AbstractListQuestionData : QuestionnaireActive
    {
        private IList<ValidationCondition> validationConditions;

        public AbstractListQuestionData()
        {
            this.validationConditions = new List<ValidationCondition>();
        }
        public string ConditionExpression { get; set; }
        public bool HideIfDisabled { get; set; }
        public QuestionProperties Properties { get; set; }
        public string Instructions { get; set; }

        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public string StataExportCaption { get; set; }
        public string VariableLabel { get; set; }
        public int? MaxAnswerCount { get; set; }
        [Obsolete("KP-6647, v5.6")]
        public string ValidationExpression { get; set; }
        [Obsolete("KP-6647, v5.6")]
        public string ValidationMessage { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public IList<ValidationCondition> ValidationConditions
        {
            get { return this.validationConditions.ConcatWithOldConditionIfNotEmpty(this.ValidationExpression, this.ValidationMessage); }
            set { this.validationConditions = value; }
        }
    }
}
