using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public abstract class AbstractQuestionUpdated : QuestionnaireActive
    {
        private IList<ValidationCondition> validationConditions;

        public AbstractQuestionUpdated()
        {
            this.validationConditions = new List<ValidationCondition>();
        }

        public Guid QuestionId { get; set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string Instructions { get; set; }
        public QuestionProperties Properties { get; set; }
        public string Title { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        [Obsolete("KP-6647")]
        public string ValidationExpression { get; set; }
        [Obsolete("KP-6647")]
        public string ValidationMessage { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public IList<ValidationCondition> ValidationConditions
        {
            get { return this.validationConditions.ConcatWithOldConditionIfNotEmpty(this.ValidationExpression, this.ValidationMessage); }
            set { this.validationConditions = value; }
        }
    }
}
