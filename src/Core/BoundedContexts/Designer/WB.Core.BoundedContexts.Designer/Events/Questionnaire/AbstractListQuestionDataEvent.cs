﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractListQuestionDataEvent : QuestionnaireActiveEvent
    {
        private IList<ValidationCondition> validationConditions;

        public AbstractListQuestionDataEvent()
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
