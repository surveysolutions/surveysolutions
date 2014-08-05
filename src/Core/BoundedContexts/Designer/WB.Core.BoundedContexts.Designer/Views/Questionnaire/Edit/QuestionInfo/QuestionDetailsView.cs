using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public abstract class QuestionDetailsView : DescendantItemView
    {
        public string EnablementCondition { get; set; }

        public bool IsPreFilled { get; set; }

        public string Instructions { get; set; }

        public bool IsMandatory { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string VariableName { get; set; }

        public string VariableLabel { get; set; }
        
        public string Title { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public abstract QuestionType Type { get; set; }
    }
}