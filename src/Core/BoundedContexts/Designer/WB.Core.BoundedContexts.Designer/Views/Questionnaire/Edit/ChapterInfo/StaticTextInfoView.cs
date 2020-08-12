using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class StaticTextInfoView : IQuestionnaireItem
    {
        public StaticTextInfoView(string itemId, string? text, string? attachmentName, bool hasCondition, bool hasValidation)
        {
            ItemId = itemId;
            Text = text;
            AttachmentName = attachmentName;
            HasCondition = hasCondition;
            HasValidation = hasValidation;
        }

        public string ItemId { get; set; }
        public ChapterItemType ItemType { get { return ChapterItemType.StaticText; }}

        public string? Text { get; set; }
        public string? AttachmentName { get; set; }

        public bool HasCondition { get; set; }
        public bool HasValidation { get; set; }

        public List<IQuestionnaireItem> Items
        {
            get
            {
                return new List<IQuestionnaireItem>(0);
            }
            set
            {
                // do nothing
            }
        }
    }
}
