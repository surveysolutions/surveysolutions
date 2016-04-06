using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class StaticTextDetailsView : DescendantItemView
    {
        public StaticTextDetailsView()
        {
            this.ValidationConditions = new List<ValidationCondition>();
        }

        public string Text { get; set; }

        public string AttachmentName { get; set; }

        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public IList<ValidationCondition> ValidationConditions { get; set; }
    }
}