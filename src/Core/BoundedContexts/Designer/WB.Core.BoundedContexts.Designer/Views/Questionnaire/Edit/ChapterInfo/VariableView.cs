using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class VariableView : IQuestionnaireItem
    {
        public string ItemId { get; set; }

        public VariableData VariableData { get; set; }

        public ChapterItemType ItemType => ChapterItemType.Variable;

        public List<IQuestionnaireItem> Items
        {
            get { return new List<IQuestionnaireItem>(); }
            set { }
        }
    }
}