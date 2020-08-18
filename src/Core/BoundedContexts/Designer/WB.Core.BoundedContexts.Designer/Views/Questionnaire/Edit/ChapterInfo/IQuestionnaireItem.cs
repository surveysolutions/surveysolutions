using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public interface IQuestionnaireItem
    {
        string ItemId { get; }
        ChapterItemType ItemType { get; }

        List<IQuestionnaireItem> Items { get; set; }
        bool HasCondition { get; set; }
        bool HideIfDisabled { get; set; }
    }
}
