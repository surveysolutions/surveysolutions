using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public abstract class QuestionDetailsView : DescendantItemView
    {
        protected QuestionDetailsView()
        {
            this.ValidationConditions = new List<ValidationCondition>();
        }

        public string EnablementCondition { get; set; }

        public bool IsPreFilled { get; set; }

        public string Instructions { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string VariableName { get; set; }

        public string VariableLabel { get; set; }

        public string Title { get; set; }

        public IList<ValidationCondition> ValidationConditions { get; set; } 

        public abstract QuestionType Type { get; set; }
    }
}