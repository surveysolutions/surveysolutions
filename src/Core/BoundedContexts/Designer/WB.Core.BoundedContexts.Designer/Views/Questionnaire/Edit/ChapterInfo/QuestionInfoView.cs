using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class QuestionInfoView : IQuestionnaireItem, INameable
    {
        public string ItemId { get; set; } = String.Empty;
        public ChapterItemType ItemType { get {return ChapterItemType.Question;}}

        public string? Title { get; set; }

        public string Variable { get; set; } = String.Empty;
        public string? LinkedToQuestionId { get; set; }
        public string? LinkedToRosterId { get; set; }
        public string? LinkedFilterExpression { get; set; }
        public QuestionType Type { get; set; }

        public bool HasCondition { get; set; }
        public bool HideIfDisabled { get; set; }

        public bool HasValidation { get; set; }
        
        public bool IsCritical { get; set; }

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
